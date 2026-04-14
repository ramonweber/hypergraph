"""
Setup script for hypergraph library
Install in editable mode: pip install -e .

This maps the repository root to the 'hypergraph' namespace, so imports work as:
    from hypergraph.api.lib.tools import RGL
"""
from setuptools import setup

setup(
    name="hypergraph",
    version="0.1.0",
    description="Hypergraph library with C# DLL integration",
    author="Hesam Salehipour",
    packages=[
        "hypergraph",
        "hypergraph.api",
        "hypergraph.api.lib",
    ],
    package_dir={
        "hypergraph": ".",
        "hypergraph.api": "api",
        "hypergraph.api.lib": "api/lib",
    },
    python_requires=">=3.7",
    install_requires=[
        "pythonnet",
        "python-dotenv",
        "pydantic",
    ],
    include_package_data=True,
)
