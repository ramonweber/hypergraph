from typing import Literal, Generic, TypeVar, ClassVar, Optional, Type
import logging
import json


from fastapi import FastAPI
from pydantic import BaseModel, model_validator, Field, field_serializer

from api.lib.tools import RGL, load_db, BaseApartment, FitReferenceRequest
from System import Collections
from System.Text.Json import JsonSerializer

logging.basicConfig(level=logging.INFO)
logger = logging.getLogger(__name__)

db = load_db()


"""
Create the API

To serve, run uvicorn api.main:api --reload
Visit http://localhost:8000/docs to see the auto-docs and try out the addition endpoint
"""
api = FastAPI()


@api.get("/")
def root():
    return {"message": "Hello world!"}


@api.get("/add")
def add(a: int, b: int):
    from FloorPlanToolsLibrary import Analyzer

    analyzer = Analyzer()
    result = analyzer.Add(a, b)
    return result


@api.get("/test/")
def test_build():
    apt = BaseApartment()
    apt.plot_bounds()

    return {"message": "sucess"}


@api.get("/apt/{apt_id}")
def get_apt(apt_id: int):
    apt = db[apt_id]
    return json.loads(JsonSerializer.Serialize(apt))


@api.get("/apt/{apt_id}/{parameter}")
def get_apt_parameter(apt_id: int, parameter: str):
    val = getattr(db[apt_id], parameter)
    return val


@api.post("/fit/reference")
def fit_from_custom(
    body: FitReferenceRequest,
):
    sources = body.sources.objs
    apt = body.target.obj
    apt_list = RGL.Apartment.createApartmentsFromReference(
        sources,
        apt,
    )
    apt_meshes = []
    for apt in [apt_list[i] for i in range(apt_list.Count)]:
        mesh = json.loads(RGL.NMesh.serializeNMesh(apt.rooms))
        apt_meshes.append(mesh)

    return apt_meshes


@api.post("/fit/db")
def fit_from_db(
    target: BaseApartment,
    building_id: str = "default",
    apartment_id: int = 0,
    areaFilter: float = 0.15,
    cullAccess: bool = True,
    sortByShape: bool = True,
):
    apt = target.obj
    apt_list = RGL.Apartment.fitApartment(
        building_id,
        apartment_id,
        db,
        apt,
        areaFilter,
        cullAccess,
        sortByShape,
    )

    apt_meshes = []
    for apt in [apt_list[i] for i in range(apt_list.Count)]:
        mesh = json.loads(RGL.NMesh.serializeNMesh(apt.rooms))
        apt_meshes.append(mesh)

    return apt_meshes
