# Third-Party Notices

The source and binaries in this repository incorporate the following third-party
components. Redistributed license texts are provided in [`licenses/`](licenses/)
and are also included in each binary release package.

| Component | Version | License | Integrated In |
| --- | --- | --- | --- |
| OpenCV | 5.0.0 | Apache 2.0 | `lw.OpenCVDNN.PPOCR.dll` |
| cpp-base64 | 2.rc.08 | zlib-style | `lw.OpenCVDNN.PPOCR.HttpServer.exe` |
| cpp-httplib | 0.48.0 (Win7 patched) | MIT | `lw.OpenCVDNN.PPOCR.HttpServer.exe` |
| spdlog | 1.17.0 | MIT | `lw.OpenCVDNN.PPOCR.HttpServer.exe` |
| fmt (bundled by spdlog) | bundled with spdlog | MIT | `lw.OpenCVDNN.PPOCR.HttpServer.exe` |
| nlohmann/json | 2.0.0 | MIT | `lw.OpenCVDNN.PPOCR.HttpServer.exe` |

## OpenCV 5.0.0

<https://opencv.org/>

Licensed under the Apache License, Version 2.0.
<http://www.apache.org/licenses/LICENSE-2.0>

## cpp-base64

<https://github.com/ReneNyffenegger/cpp-base64>

Licensed under the zlib-style terms included with the upstream source.
Copyright (c) 2004-2017, 2020, 2021 René Nyffenegger.

## cpp-httplib

<https://github.com/yhirose/cpp-httplib>

Licensed under the MIT License. Copyright (c) Yuji Hirose and contributors.

The HTTP server uses a patched copy with the following changes for Windows 7
compatibility:
- Disabled Windows 10 async DNS code path
- `CreateFile2` replaced with `CreateFileW`
- App file mapping API replaced with Win7 file mapping API

## spdlog

<https://github.com/gabime/spdlog>

Licensed under the MIT License. Copyright (c) 2016 Gabi Melman.

## fmt

<https://github.com/fmtlib/fmt>

Bundled by spdlog and licensed under the MIT License.

## nlohmann/json

<https://github.com/nlohmann/json>

Licensed under the MIT License. Copyright (c) 2013-2016 Niels Lohmann.

## PaddleOCR Models

The ONNX models in the `inference/` directories are derived from
[PaddleOCR](https://github.com/PaddlePaddle/PaddleOCR), which is licensed under
the Apache License 2.0. Copyright (c) 2020 PaddlePaddle Authors.
