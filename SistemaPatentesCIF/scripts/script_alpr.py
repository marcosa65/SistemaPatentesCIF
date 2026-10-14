import json
from fast_alpr import ALPR

# Cargar una sola vez al iniciar el módulo
_alpr = ALPR( 
    detector_model="yolo-v9-t-384-license-plate-end2end", 
    ocr_model="cct-xs-v2-global-model"
    )

# Llamada al engine para detectar placas, devuelve un json 
def detect_plate(ruta_imagen:str) -> str:
    try:
        resultados = _alpr.predict(ruta_imagen)
        plates = []
        for r in resultados:
            plates.append({
                "Text": r.ocr.text,
                "Confidence": r.ocr.confidence[0],
                "Region": r.ocr.region,
                "RegionConfidence": r.ocr.region_confidence,
                "x1": r.detection.bounding_box.x1,
                "y1": r.detection.bounding_box.y1,
                "x2": r.detection.bounding_box.x2,
                "y2": r.detection.bounding_box.y2
            })
        return json.dumps({"Success": True, "Plates": plates})
    except Exception as ex:
        return json.dumps({"Success": False, "Error": str(ex)})