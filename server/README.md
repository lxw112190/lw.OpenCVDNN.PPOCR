# lw.OpenCVDNN.PPOCR.HttpServer

A lightweight HTTP REST API server for PP-OCR inference, targeting Windows 7 SP1
and later. Runs as a console application or a Windows service.

## Architecture

```text
Browser / REST Client
        │
        ▼
┌─────────────────────────────┐
│  lw.OpenCVDNN.PPOCR         │
│  .HttpServer.exe             │
│                              │
│  • httplib (Win7-patched)   │
│  • spdlog (logging)          │
│  • cpp-base64 (decoding)     │
│  • nlohmann/json             │
└──────────┬──────────────────┘
           │  C ABI (ppocr_api.h)
           ▼
┌─────────────────────────────┐
│  lw.OpenCVDNN.PPOCR.dll     │
│  (binary-only, from release)│
└─────────────────────────────┘
```

## Prerequisites

- Visual Studio 2019 (v142 toolset) or later, with C++ desktop workload
- Windows 10/11 SDK
- CMake 3.20+ (for CMake builds)

## Quick Build (Visual Studio)

1. Open `lw.OpenCVDNN.PPOCR.HttpServer.sln` (or create a solution with the
   `.vcxproj` in `server/projects/`).

2. Set the solution directory to the repository root.

3. Ensure the pre-built `lw.OpenCVDNN.PPOCR.dll` and
   `lw.OpenCVDNN.PPOCR.lib` are available. By default the project looks in:
   - x64: `$(SolutionDir)release\win-x64\sdk\native\`
   - x86: `$(SolutionDir)release\win-x86\sdk\native\`

   Place the DLL and .lib there, or update `AdditionalLibraryDirectories` in
   the `.vcxproj`.

4. Build with `Release | x64` (or `Release | Win32` for x86).

5. Copy the DLL and model files alongside the output EXE, or run from the
   release package directory.

## CMake Build

```bat
cd server
mkdir build && cd build

cmake .. -G "Visual Studio 16 2019" -A x64

cmake --build . --config Release
```

The default CMake paths already point to the bundled x64 SDK.

For x86:

```bat
cmake .. -G "Visual Studio 16 2019" -A Win32 ^
    -DPPOCR_LIB_DIR=../../release/win-x86/sdk/native ^
    -DPPOCR_DLL_DIR=../../release/win-x86
```

The CMake project automatically enables the x86 low-memory settings and SSE2
target when `-A Win32` is selected.

## Usage

```bat
lw.OpenCVDNN.PPOCR.HttpServer.exe --host 127.0.0.1 --port 8080
```

Open `http://127.0.0.1:8080/` in a browser.

The server defaults to `0.0.0.0` and has no built-in authentication or TLS.
Use the loopback binding above for local-only access; use a firewall and an
authenticated HTTPS reverse proxy when exposing it beyond a trusted LAN.

### CLI Options

| Option | Default | Description |
| --- | --- | --- |
| `--host` | `0.0.0.0` | Bind address |
| `--port` | `8080` | Listen port |
| `--mode` | `both` | `ocr`, `rec`, or `both`; `rec` loads only recognition assets |
| `--det_model` | `inference/PP-OCRv6_tiny_det.onnx` | Detection model |
| `--rec_model` | `inference/PP-OCRv6_tiny_rec.onnx` | Recognition model |
| `--rec_dict` | `inference/PP-OCRv6_tiny_rec_dict.txt` | Recognition dictionary |
| `--limit_side_len` | `960` | Max side length for detection |
| `--det_db_thresh` | `0.3` | Detection threshold |
| `--det_db_box_thresh` | `0.6` | Detection box threshold |
| `--det_db_unclip_ratio` | `1.5` | Unclip ratio |
| `--rec_batch_num` | `8` | Recognition batch size |
| `--rec_img_h` | `48` | Recognition input height |
| `--rec_img_w` | `320` | Recognition input width |
| `--rec_predictor_num` | `4` | Recognition predictor count (x86: 1) |
| `--cpu_threads` | `0` | CPU threads (0 = OpenCV default; x86: 2) |
| `--service` | _(off)_ | Run as Windows service (SCM only) |

### API

Full OCR (detection and recognition):

```http
POST /api/ocr
Content-Type: application/json

{"imageBase64": "data:image/jpeg;base64,..."}
```

Recognition of one already-cropped text line:

```http
POST /api/recognize
Content-Type: application/json

{"imageBase64": "data:image/jpeg;base64,..."}
```

Recognition of fixed regions without detection/classification:

```http
POST /api/recognize-rois
Content-Type: application/json

{
  "imageBase64": "...",
  "rois": [
    {"id": 1, "x": 100, "y": 80, "width": 240, "height": 48, "rotation": 0},
    {
      "id": 2,
      "points": [
        {"x": 100, "y": 150}, {"x": 340, "y": 140},
        {"x": 345, "y": 190}, {"x": 105, "y": 200}
      ],
      "rotation": 0
    }
  ]
}
```

Quadrilateral points are ordered top-left, top-right, bottom-right,
bottom-left. Coordinates refer to decoded source-image pixels. Use
`--mode rec` for a recognition-only deployment that does not load the detector.

```http
GET /health
```

### Windows Service

Run as Administrator:

```bat
deploy\install_service.bat
```

## Directory Structure

```text
server/
├── README.md                          # This file
├── CMakeLists.txt                     # CMake build
├── main.cpp                           # Server entry point
├── lw.OpenCVDNN.PPOCR.HttpServer.rc   # Windows resource
├── resource.h                         # Resource IDs
├── src/
│   └── json.cpp                       # nlohmann/json (bundled)
├── third_party/
│   └── httplib.h                      # cpp-httplib, Win7-patched
├── projects/
│   └── lw.OpenCVDNN.PPOCR.HttpServer.vcxproj  # VS 2019 project
├── www/
│   └── index.html                     # Web UI
└── deploy/
    ├── install_service.bat
    └── uninstall_service.bat
```

## Third-Party Dependencies

| Component | Location | Notes |
| --- | --- | --- |
| cpp-httplib | `server/third_party/httplib.h` | Win7-patched copy (CreateFile2→CreateFileW, etc.) |
| cpp-base64 | `../third_party/cpp-base64/` | Shared with DLL build |
| spdlog | `../third_party/spdlog/include/` | Header-only logging |
| nlohmann/json | `server/src/json.cpp` | Bundled v2.0.0 |
| ppocr_api.h | `../include/ppocr_api.h` | C ABI header for lw.OpenCVDNN.PPOCR.dll |

## License

MIT License. See [../LICENSE](../LICENSE).
