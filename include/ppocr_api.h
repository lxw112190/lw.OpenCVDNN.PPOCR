#pragma once

#include <stdint.h>

#if defined(_WIN32)
#  if defined(PPOCR_NATIVE_EXPORTS)
#    define PPOCR_API __declspec(dllexport)
#  else
#    define PPOCR_API __declspec(dllimport)
#  endif
#  define PPOCR_CALL __cdecl
#else
#  define PPOCR_API
#  define PPOCR_CALL
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef void* ppocr_handle;

/*
 * A quadrilateral text region in decoded-image pixel coordinates.
 * Points must be ordered: top-left, top-right, bottom-right, bottom-left.
 * rotation is applied clockwise after perspective cropping and must be
 * one of 0, 90, 180 or 270.
 */
typedef struct ppocr_roi {
    int32_t struct_size;
    int32_t id;
    int32_t x1;
    int32_t y1;
    int32_t x2;
    int32_t y2;
    int32_t x3;
    int32_t y3;
    int32_t x4;
    int32_t y4;
    int32_t rotation;
} ppocr_roi;

/*
 * UTF-16 configuration for Windows callers. Initialize it with ppocr_config_init.
 * det_model_path may be null for recognition-only handles. rec_model_path and
 * rec_dict_path must either both be provided or both be null.
 */
typedef struct ppocr_config_w {
    int32_t struct_size;
    const wchar_t* det_model_path;
    const wchar_t* rec_model_path;
    const wchar_t* rec_dict_path;
    const wchar_t* cls_model_path;

    int32_t limit_side_len;
    double det_db_thresh;
    double det_db_box_thresh;
    double det_db_unclip_ratio;
    int32_t use_dilation;

    int32_t use_angle_cls;
    double cls_thresh;
    int32_t cls_batch_num;

    int32_t rec_batch_num;
    int32_t rec_img_h;
    int32_t rec_img_w;
    int32_t rec_predictor_num;
    int32_t cpu_threads;
} ppocr_config_w;

PPOCR_API void PPOCR_CALL ppocr_config_init(ppocr_config_w* config);
PPOCR_API void PPOCR_CALL ppocr_roi_init(ppocr_roi* roi);

PPOCR_API int32_t PPOCR_CALL ppocr_create_w(
    const wchar_t* det_model_path,
    const wchar_t* rec_model_path,
    const wchar_t* rec_dict_path,
    int32_t cpu_threads,
    ppocr_handle* handle,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_create_ex_w(
    const ppocr_config_w* config,
    ppocr_handle* handle,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_ocr_encoded(
    ppocr_handle handle,
    const uint8_t* encoded_image,
    int32_t encoded_size,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_ocr_bgr(
    ppocr_handle handle,
    const uint8_t* pixels,
    int32_t width,
    int32_t height,
    int32_t channels,
    int32_t stride,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

/* Recognize one already-cropped text-line image without detection/classification. */
PPOCR_API int32_t PPOCR_CALL ppocr_recognize_encoded(
    ppocr_handle handle,
    const uint8_t* encoded_image,
    int32_t encoded_size,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_recognize_bgr(
    ppocr_handle handle,
    const uint8_t* pixels,
    int32_t width,
    int32_t height,
    int32_t channels,
    int32_t stride,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

/*
 * Recognize multiple encoded, already-cropped text-line images in input order.
 * encoded_images and encoded_sizes must both contain image_count entries.
 */
PPOCR_API int32_t PPOCR_CALL ppocr_recognize_encoded_batch(
    ppocr_handle handle,
    const uint8_t* const* encoded_images,
    const int32_t* encoded_sizes,
    int32_t image_count,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

/* Decode/copy the source image once, crop all ROIs, then recognize in a batch. */
PPOCR_API int32_t PPOCR_CALL ppocr_recognize_rois_encoded(
    ppocr_handle handle,
    const uint8_t* encoded_image,
    int32_t encoded_size,
    const ppocr_roi* rois,
    int32_t roi_count,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

PPOCR_API int32_t PPOCR_CALL ppocr_recognize_rois_bgr(
    ppocr_handle handle,
    const uint8_t* pixels,
    int32_t width,
    int32_t height,
    int32_t channels,
    int32_t stride,
    const ppocr_roi* rois,
    int32_t roi_count,
    char** utf8_json,
    int32_t* json_size,
    wchar_t* error_message,
    int32_t error_capacity);

/*
 * utf8_json is allocated by the DLL and must be released with ppocr_free.
 * A handle serializes its own OCR calls. Do not call ppocr_destroy while a call is active.
 */
PPOCR_API void PPOCR_CALL ppocr_free(void* memory);
PPOCR_API void PPOCR_CALL ppocr_destroy(ppocr_handle handle);
PPOCR_API const char* PPOCR_CALL ppocr_get_version(void);

#ifdef __cplusplus
}
#endif
