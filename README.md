## SM64DSe-Ultimate

This is a special version of the editor offering a *RestAPI* to interact with the ROM.

The idea is to re-write the internal logic of the editor, to separate the UI and the logic to expose this logic through an API.

## Installation

You can download the latest version from [releases section](https://github.com/Gota7/SM64DSe-Ultimate/releases/latest) and download the `release.zip`.

Simply unzip the content in a folder, and launch `SM64DSe.exe`.


## Features

To start the APi without the UI use the following command
````shell
SM64DSe.exe <path-to-.nds> no-ui console
````

> The `console` argument will make a console visible with the logs of the program.

### overlays

- [x] List overlays: `GET /api/overlays/`
- [x] Get overlays by levelID: `GET /api/overlays/?levelId={levelId}`
- [x] Get overlays data by ID: `GET /api/overlays/{ovlId}`
- [x] Get number of overlays: `GET /api/overlays/count`
- [ ] Remove overlay
- [ ] Insert overlay
- [ ] Replace overlay
- [ ] Update overlay info

### filesystem

- [x] List files: `GET /api/files/`
- [x] List directories: `GET /api/files/directories`
- [ ] Get File data
- [ ] Create Directory
- [ ] Insert file
- [ ] Delete File
- [ ] 

### patches

- [ ] 

### textures

- [ ] 

### levels

- [ ]

## API Usage

When started with a .nds file as first argument. An API at `localhost:8888/api` will be exposed.

## `/api/`

Will return the ROM Metadata

````json
{
    "m_IsROMFolder": false,
    "m_ROMBasePath": null,
    "m_ROMPatchPath": null,
    "m_ROMConversionPath": null,
    "m_ROMBuildPath": null,
    "m_ROMPath": "<path-to-.nds>"
}
````

## `/api/version`

Will return the ROM Version

```json
{
  "version": "EUR"
}
```

## `/api/overlays`

TODO
