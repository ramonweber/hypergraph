# TODO: switch to 3.9
# TODO: see alternate mono install instructions here: https://github.com/michaelosthege/pythonnet-docker/tree/main
FROM python:3.9

# Install Mono
RUN apt-get update \
	&& apt-get install -y gnupg ca-certificates \
	&& apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF \
	&& echo "deb https://download.mono-project.com/repo/debian stable-buster main" | tee /etc/apt/sources.list.d/mono-official-stable.list \
	&& apt-get update \
	&& apt-get install -y mono-complete \ 
	&& rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src/app

# Install python reqs
COPY requirements.txt ./
RUN pip install -r requirements.txt

# COPY the local DLL to the container
# TODO: use an arg/dynamic path
COPY dlls/ dlls/

# Copy Datafiles
COPY database/ database/

# Copy the rest of the files
COPY api/ api/

# Expose the port for the api
EXPOSE 8000

ENV PYTHONPATH "${PYTHONPATH}:/usr/src/app"

ENTRYPOINT ["uvicorn", "api.main:api", "--host", "0.0.0.0"]

# To run with tunnel, use: 
# docker run -p 8000:8000 <image-name>

