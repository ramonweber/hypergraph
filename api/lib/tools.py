from typing import ClassVar, TypeVar, Generic, Optional, Type, Literal, Union
import clr
from pathlib import Path
import os
from System import Collections

from dotenv import load_dotenv

"""
Load .dll into clr and import the namespace
"""
load_dotenv()

# If Env variable is not set, look in
# default output folder for visual studio build
# Use __file__ to get path relative to this script, not cwd
hypergraph_root = Path(__file__).resolve().parent.parent.parent  # api/lib/tools.py -> hypergraph root
reqs_path = hypergraph_root / "dlls" / "reqs"
libs_path = hypergraph_root / "dlls" / "main"
for dll_path in os.listdir(reqs_path):
    # get rid of dll extension
    if dll_path.endswith(".dll"):
        dll_path = reqs_path / dll_path[:-4]
        clr.AddReference(str(dll_path))

for dll_path in os.listdir(libs_path):
    if dll_path.endswith(".dll"):
        dll_path = libs_path / dll_path[:-4]
        clr.AddReference(str(dll_path))


import RGeoLib as RGL
from System.Text.Json import JsonSerializer
from pydantic import BaseModel, field_serializer, model_validator, field_validator
import logging

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)


RoomTypes = Literal["bed", "living", "kitchen", "bath", "extra", "foyer"]


def to_rgl_list(data, datatype):
    rgl_list = Collections.Generic.List[datatype]()
    for obj in data:
        rgl_list.Add(obj.obj)
    return rgl_list


class RGLTWrapper(BaseModel, validate_assignment=True):
    RGLT: ClassVar[any]

    @property
    def obj(self):
        return None


GenericRGLT = TypeVar("GenericRGLT", bound=RGLTWrapper)


class RGLList(BaseModel, Generic[GenericRGLT]):
    data: list[GenericRGLT] = []
    dtype: Optional[Union[Type, str]] = None

    # TODO: better/more consistent serialization/representation in schema
    @field_serializer("dtype")
    def serialize_dtype(self, v):
        return v.__name__

    @model_validator(mode="after")
    def validate_model(self) -> "RGLList":
        if len(self.data) == 0:
            assert self.data is not None, "dtype must be set if data is empty"
            assert issubclass(
                self.dtype, RGLTWrapper
            ), f"dtype must be a subclass of RGLTWrapper, but got {self.dtype}"
        else:
            self.dtype = type(self[0])

        return self

    @property
    def objs(self) -> Collections.Generic.List:
        rgl_list = Collections.Generic.List[self.dtype.RGLT]()
        for obj in self:
            rgl_list.Add(obj.obj)

        return rgl_list

    def __iter__(self):
        return iter(self.data)

    def __getitem__(self, ix):
        return self.data[ix]


class Pt(RGLTWrapper):
    RGLT: ClassVar[type] = RGL.Vec3d
    X: float = 0.0
    Y: float = 0.0
    Z: float = 0.0

    @model_validator(mode="before")
    def validate_pt(cls, v):
        if "loc" in v:
            assert len(v["loc"]) in [2, 3], "loc must be a 2 or 3-tuple"
            v["X"] = v["loc"][0]
            v["Y"] = v["loc"][1]
            if len(v["loc"]) == 3:
                v["Z"] = v["loc"][2]
        return v

    @property
    def loc(self):
        return (self.X, self.Y, self.Z)

    @property
    def obj(self) -> RGL.Vec3d:
        return RGL.Vec3d(x=self.X, y=self.Y, z=self.Z)


class Line(RGLTWrapper):
    RGLT: ClassVar[type] = RGL.NLine
    start: Pt = Pt(loc=(0.0, 0.0))
    end: Pt = Pt(loc=(0.0, 10.0))

    @property
    def obj(self) -> RGL.NLine:
        return RGL.NLine(self.start.obj, self.end.obj)


class Face(RGLTWrapper):
    RGLT: ClassVar[type] = RGL.NFace
    corners: RGLList[Pt] = RGLList[Pt](
        data=[
            Pt(loc=(0.0, 0.0)),
            Pt(loc=(0.0, 10.0)),
            Pt(loc=(10, 10)),
            Pt(loc=(10, 0)),
        ]
    )
    merge_id: Optional[str] = None

    @field_validator("corners", mode="before")
    def validate_corners(cls, v):
        if isinstance(v, list):
            v = RGLList[Pt](data=v, dtype=Pt)
        return v

    @model_validator(mode="before")
    def validate_lists(cls, v):
        if "corners" in v:
            if isinstance(v["corners"], list):
                v["corners"] = RGLList[Pt](data=v["corners"], dtype=Pt)
        return v

    @property
    def obj(self) -> RGL.NFace:
        pts = self.corners.objs
        face = RGL.NFace(pts)
        if self.merge_id is not None:
            face.merge_id = self.merge_id
        return face


class RoomMesh(RGLTWrapper):
    RGLT: ClassVar[type] = RGL.NMesh
    faceList: RGLList[Face] = RGLList[Face](
        data=[
            Face(
                corners=RGLList[Pt](
                    data=[
                        Pt(loc=(0.0, 0.0)),
                        Pt(loc=(0.0, 10.0)),
                        Pt(loc=(4, 10)),
                        Pt(loc=(4, 0)),
                    ]
                ),
                merge_id="bed",
            ),
            Face(
                corners=RGLList[Pt](
                    data=[
                        Pt(loc=(4, 0.0)),
                        Pt(loc=(4, 10.0)),
                        Pt(loc=(10, 10)),
                        Pt(loc=(10, 0)),
                    ]
                ),
                merge_id="living",
            ),
        ]
    )

    @field_validator("faceList", mode="before")
    def validate_faceList(cls, v):
        if isinstance(v, list):
            v = RGLList[Face](data=v, dtype=Face)
        return v

    @model_validator(mode="before")
    def validate_lists(cls, v):
        if "faceList" in v:
            if isinstance(v["faceList"], list):
                v["faceList"] = RGLList[Pt](data=v["faceList"], dtype=Face)
        return v

    @property
    def obj(self) -> RGL.NMesh:
        faceList = self.faceList.objs
        mesh = RGL.NMesh(faceList)
        return mesh


class BaseApartment(RGLTWrapper):
    RGLT: ClassVar[type] = RGL.Apartment
    bounds: Face = Face(
        corners=RGLList[Pt](
            data=[
                Pt(loc=(0.0, 0.0)),
                Pt(loc=(0.0, 10.0)),
                Pt(loc=(10, 10)),
                Pt(loc=(10, 0)),
            ]
        )
    )
    facade: RGLList[Line] = RGLList[Line](
        data=[
            Line(
                a=Pt(loc=(0, 0)),
                b=Pt(loc=(0, 10)),
            )
        ]
    )
    circulation: RGLList[Line] = RGLList[Line](
        data=[
            Line(
                a=Pt(loc=(10, 0)),
                b=Pt(loc=(10, 10)),
            )
        ]
    )

    @field_validator("facade", mode="before")
    def validate_facade(cls, v):
        if isinstance(v, list):
            v = RGLList[Line](data=v, dtype=Line)
        return v

    @field_validator("circulation", mode="before")
    def validate_circulation(cls, v):
        if isinstance(v, list):
            v = RGLList[Line](data=v, dtype=Line)
        return v

    @model_validator(mode="before")
    def validate_lists(cls, v):
        for key, dtype in [("facade", Line), ("circulation", Line)]:
            if key in v:
                if isinstance(v[key], list):
                    v[key] = RGLList[dtype](data=v[key], dtype=dtype)
        return v

    @property
    def obj(self) -> RGL.Apartment:
        apt = RGL.Apartment(self.bounds.obj, self.facade.objs, self.circulation.objs)
        return apt

    def plot_bounds(self, axs=None):
        import matplotlib.pyplot as plt

        if axs is None:
            fig, axs = plt.subplots()

        pts = self.bounds.corners
        x = [pt.loc[0] for pt in pts] + [pts[0].loc[0]]
        y = [pt.loc[1] for pt in pts] + [pts[0].loc[1]]

        axs.plot(x, y)

        return axs

    def plot(self, *args, **kwargs):
        axs = self.plot_bounds(*args, **kwargs)
        return axs


class FullApartment(BaseApartment):
    rooms: RoomMesh = RoomMesh()
    doors: RGLList[Pt] = RGLList[Pt](data=[Pt(loc=(4, 5))])

    @model_validator(mode="before")
    def validate_lists(cls, v):
        for key, dtype in [
            ("doors", Pt),
        ]:
            if key in v:
                if isinstance(v[key], list):
                    v[key] = RGLList[dtype](data=v[key], dtype=dtype)
        return v

    @property
    def obj(self) -> RGL.Apartment:
        apt = RGL.Apartment(
            self.bounds.obj,
            self.rooms.obj,
            self.facade.objs,
            self.circulation.objs,
            self.doors.objs,
        )
        return apt

    def plot_rooms(self, axs=None):
        import matplotlib.pyplot as plt

        if axs is None:
            fig, axs = plt.subplots()

        for face in self.rooms.faceList:
            x = [pt.loc[0] for pt in face.corners] + [face.corners[0].loc[0]]
            y = [pt.loc[1] for pt in face.corners] + [face.corners[0].loc[1]]
            axs.plot(x, y, label=face.merge_id)

        return axs

    def plot(self, axs=None):
        axs = self.plot_bounds(axs)
        axs = self.plot_rooms(axs)

        return axs


class FitReferenceRequest(BaseModel):
    sources: RGLList[FullApartment] = RGLList[FullApartment](data=[FullApartment()])
    target: BaseApartment = BaseApartment()


def build_apartment():
    Vec3d = RGL.Vec3d
    NFace = RGL.NFace
    NLine = RGL.NLine
    NMesh = RGL.NMesh
    DataNode = RGL.DataNode
    Apartment = RGL.Apartment
    a = Vec3d(x=0, y=0, z=0)
    b = Vec3d(x=0, y=10, z=0)
    c = Vec3d(x=10, y=10, z=0)
    d = Vec3d(x=10, y=0, z=0)

    a_facade = Vec3d(x=0, y=0, z=0)
    b_facade = Vec3d(x=0, y=10, z=0)

    c_circ = Vec3d(x=10, y=10, z=0)
    d_circ = Vec3d(x=10, y=0, z=0)

    vec_list = Collections.Generic.List[Vec3d]()
    for vec in [a, b, c, d]:
        vec_list.Add(vec)

    bounds = NFace(vec_list)
    facade = NLine(a_facade, b_facade)
    circ = NLine(c_circ, d_circ)
    facade_list = Collections.Generic.List[NLine]()
    facade_list.Add(facade)
    circ_list = Collections.Generic.List[NLine]()
    circ_list.Add(circ)

    room_1_a = Vec3d(x=0, y=0, z=0)
    room_1_b = Vec3d(x=0, y=10, z=0)
    room_1_c = Vec3d(x=4, y=10, z=0)
    room_1_d = Vec3d(x=4, y=0, z=0)
    room_2_c = Vec3d(x=10, y=10, z=0)
    room_2_d = Vec3d(x=10, y=0, z=0)

    room_1_list = Collections.Generic.List[Vec3d]()
    for vec in [room_1_a, room_1_b, room_1_c, room_1_d]:
        room_1_list.Add(vec)

    room_2_list = Collections.Generic.List[Vec3d]()
    for vec in [room_1_d, room_1_c, room_2_c, room_2_d]:
        room_2_list.Add(vec)

    room_1 = NFace(room_1_list)
    room_2 = NFace(room_2_list)
    room_1.merge_id = "bed"
    room_2.merge_id = "living"

    for room in [room_1, room_2]:
        room.updateEdgeConnectivity()

    room_list = Collections.Generic.List[NFace]()
    for room in [room_1, room_2]:
        room_list.Add(room)

    room_mesh = NMesh(room_list)
    door_pt = Vec3d(x=4, y=5, z=0)
    door_list = Collections.Generic.List[Vec3d]()
    door_list.Add(door_pt)

    apartment = Apartment(
        bounds,
        room_mesh,
        facade_list,
        circ_list,
        door_list,
    )

    return apartment


def load_db():
    db = RGL.BuildingSolver.ALibrary.ReadApartmentsFromJson("database/database.json")
    return db
