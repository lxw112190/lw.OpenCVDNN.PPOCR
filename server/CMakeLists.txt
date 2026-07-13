cmake_minimum_required(VERSION 3.20)
project(lw_OpenCVDNN_PPOCR_HttpServer LANGUAGES CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)
set(CMAKE_MSVC_RUNTIME_LIBRARY "MultiThreaded$<$<CONFIG:Debug>:Debug>")

# PROJECT_ROOT is the repository root (parent of this server/ directory)
get_filename_component(PROJECT_ROOT "${CMAKE_CURRENT_SOURCE_DIR}/.." ABSOLUTE)

# ---- user-configurable paths ----
# Point these to the pre-built lw.OpenCVDNN.PPOCR release package.
set(PPOCR_LIB_DIR "${PROJECT_ROOT}/release/win-x64/sdk/native" CACHE PATH
    "Directory containing lw.OpenCVDNN.PPOCR.lib")
set(PPOCR_DLL_DIR "${PROJECT_ROOT}/release/win-x64" CACHE PATH
    "Directory containing lw.OpenCVDNN.PPOCR.dll (copied to output on build)")

add_executable(lw.OpenCVDNN.PPOCR.HttpServer
    lw.OpenCVDNN.PPOCR.HttpServer.rc
    main.cpp
    "${PROJECT_ROOT}/third_party/cpp-base64/base64.cpp"
)

target_include_directories(lw.OpenCVDNN.PPOCR.HttpServer PRIVATE
    "${CMAKE_CURRENT_SOURCE_DIR}/third_party"       # Win7-patched httplib.h
    "${PROJECT_ROOT}/include"                       # ppocr_api.h
    "${PROJECT_ROOT}/third_party/cpp-base64"        # base64.h
    "${PROJECT_ROOT}/third_party/spdlog/include"    # spdlog (header-only)
)

target_compile_definitions(lw.OpenCVDNN.PPOCR.HttpServer PRIVATE
    WINVER=0x0601
    _WIN32_WINNT=0x0601
    WIN32_LEAN_AND_MEAN
    NOMINMAX
)

if(CMAKE_SIZEOF_VOID_P EQUAL 4)
    target_compile_definitions(lw.OpenCVDNN.PPOCR.HttpServer PRIVATE
        PPOCR_X86_LOW_MEMORY=1
        PPOCR_HTTP_THREAD_POOL_COUNT=2
    )
endif()

target_compile_options(lw.OpenCVDNN.PPOCR.HttpServer PRIVATE
    /utf-8
    $<$<CONFIG:Release>:/O2;/Oi;/Gy>
    $<$<AND:$<BOOL:${MSVC}>,$<EQUAL:${CMAKE_SIZEOF_VOID_P},4>>:/arch:SSE2>
)

target_link_options(lw.OpenCVDNN.PPOCR.HttpServer PRIVATE
    $<$<CONFIG:Release>:/OPT:REF;/OPT:ICF>
    /SUBSYSTEM:CONSOLE,6.01
)

target_link_libraries(lw.OpenCVDNN.PPOCR.HttpServer PRIVATE
    "${PPOCR_LIB_DIR}/lw.OpenCVDNN.PPOCR.lib"
    ws2_32
    advapi32
    ole32
)

set_target_properties(lw.OpenCVDNN.PPOCR.HttpServer PROPERTIES
    OUTPUT_NAME "lw.OpenCVDNN.PPOCR.HttpServer"
    RUNTIME_OUTPUT_DIRECTORY "${CMAKE_BINARY_DIR}/bin"
)

# Copy DLL and web UI to output directory after build
add_custom_command(TARGET lw.OpenCVDNN.PPOCR.HttpServer POST_BUILD
    COMMAND ${CMAKE_COMMAND} -E copy_if_different
        "${PPOCR_DLL_DIR}/lw.OpenCVDNN.PPOCR.dll"
        "$<TARGET_FILE_DIR:lw.OpenCVDNN.PPOCR.HttpServer>"
    COMMAND ${CMAKE_COMMAND} -E copy_directory
        "${CMAKE_CURRENT_SOURCE_DIR}/www"
        "$<TARGET_FILE_DIR:lw.OpenCVDNN.PPOCR.HttpServer>/www"
)
