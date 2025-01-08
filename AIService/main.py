from flask import Flask, request, jsonify, send_from_directory
import cv2
import mediapipe as mp
import numpy as np
import os
import random

# Flask uygulaması
app = Flask(__name__)

# Mediapipe yüz mesh modeli
mp_face_mesh = mp.solutions.face_mesh
face_mesh = mp_face_mesh.FaceMesh(static_image_mode=True, max_num_faces=1)

# Mediapipe yüz tespiti modeli
mp_face_detection = mp.solutions.face_detection

# Yüz analizi fonksiyonu
# Görseli okur ve RGB renk formatına dönüştürür.
# Yüz mesh modeli ile yüz özelliklerini çıkarır.
# Çene noktalarının koordinatlarını analiz eder.
# Yüzün genişlik/yükseklik oranını hesaplar ve oranlara göre yüz şekli sınıflandırması yapar.
def analyze_face(image_path):
    image = cv2.imread(image_path)
    ih, iw, _ = image.shape  # Görselin yüksekliği ve genişliği
    rgb_image = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
    results = face_mesh.process(rgb_image)

    if not results.multi_face_landmarks:
        return "Yüz tespit edilemedi", None

    face_landmarks = results.multi_face_landmarks[0]
    jaw_points = [face_landmarks.landmark[i] for i in range(0, 17)]  # Çene noktaları

    # Normalleştirilmiş koordinatları piksele dönüştür
    jaw_x = [lm.x * iw for lm in jaw_points]
    jaw_y = [lm.y * ih for lm in jaw_points]

    # Yüz genişliği ve yüksekliğini hesapla
    face_width = abs(jaw_x[-1] - jaw_x[0])  # Sol ve sağ çene arası genişlik
    face_height = max(jaw_y) - min(jaw_y)   # Çene altı ve yüz üstü yüksekliği
    aspect_ratio = face_width / face_height * 100  # Genişlik/Yükseklik oranı

    print(f"Face Width (pixels): {face_width}, Face Height (pixels): {face_height}, Aspect Ratio: {aspect_ratio}")

    # Daha fazla yüz şekli sınıflandırması
    if aspect_ratio >= 1.5:
        return "Yuvarlak", image
    elif 1.3 <= aspect_ratio < 1.5:
        return "Oval", image
    elif 1.1 <= aspect_ratio < 1.3:
        return "Uzun", image
    else:
        return "Kare", image  # Eğer oran çok düşükse "Kare" yüz şekli

# Saç modelini yüz üzerine bindirme fonksiyonu
def overlay_hair(image_path, hair_model_path):
    image = cv2.imread(image_path, cv2.IMREAD_UNCHANGED)
    hair = cv2.imread(hair_model_path, cv2.IMREAD_UNCHANGED)

    with mp_face_detection.FaceDetection(min_detection_confidence=0.5) as face_detection:
        image_rgb = cv2.cvtColor(image, cv2.COLOR_BGR2RGB)
        results = face_detection.process(image_rgb)

        if not results.detections:
            print("Yüz tespit edilemedi!")
            return None

        ih, iw, _ = image.shape
        mask = np.ones((ih, iw), dtype=np.uint8) * 255

        for detection in results.detections:
            bboxC = detection.location_data.relative_bounding_box
            x, y, w, h = int(bboxC.xmin * iw), int(bboxC.ymin * ih), int(bboxC.width * iw), int(bboxC.height * ih)

            points = np.array([
                [x - 40, y - 30],
                [x + w + 40, y - 30],
                [x + w + 50, y + h // 3],
                [x + w + 150, y + h + 200],
                [x - 150, y + h + 200],
                [x - 70, y + h // 3]
            ], np.int32)

            cv2.fillPoly(mask, [points], 0)

        if image.shape[2] == 3:
            image = cv2.cvtColor(image, cv2.COLOR_BGR2BGRA)
        image[:, :, 3][mask == 255] = 0

        hair_width = int(w * 1.62)
        hair_height = int(h * 1)
        resized_hair = cv2.resize(hair, (hair_width, hair_height))

        hair_x = x - int((hair_width - w + 10) / 1.9)
        hair_y = y - int(hair_height * 0.5)
        image = overlay_image_alpha(image, resized_hair, (hair_x, hair_y))

    return image


#Saç modelini yüz görseline alfa kanalı (şeffaflık) ile ekler.
def overlay_image_alpha(img, overlay, position):
    x, y = position
    h, w = overlay.shape[:2]

    for i in range(h):
        for j in range(w):
            if y + i >= img.shape[0] or x + j >= img.shape[1] or y + i < 0 or x + j < 0:
                continue
            alpha = overlay[i, j, 3] / 255.0
            if alpha > 0:
                img[y + i, x + j, :3] = (
                    alpha * overlay[i, j, :3] + (1 - alpha) * img[y + i, x + j, :3]
                )
                img[y + i, x + j, 3] = 255

    return img

# Yüz şekline göre önerilen saç modelleri ve renkleri
HAIR_RECOMMENDATIONS = {
    "Oval": [
        {"model": "styles/black_hair.png", "color": "#000000"},
        {"model": "styles/classic2_black_wave_hair.png", "color": "#3A3A3A"},
        {"model": "styles/galactic_black_hair.png", "color": "#2F2F4F"},
    ],
    "Yuvarlak": [
       
        {"model": "styles/black_hair.png", "color": "#000000"},
        {"model": "styles/brown_hair.png", "color": "#8B4513"},
        {"model": "styles/classic2_black_wave_hair.png", "color": "#3A3A3A"},
        {"model": "styles/light_brown_spiky_hair.png", "color": "#D2B48C"},
        {"model": "styles/messy_brown_hair.png", "color": "#8B4513"},
    ],
    "Kare": [
        {"model": "styles/modern_black_side_part.png", "color": "#000000"},
        {"model": "styles/modern_layered_black_hair.png", "color": "#2B2B2B"},
        {"model": "styles/slick_black_hair.png", "color": "#1C1C1C"},
        {"model": "styles/slick_brown_hair.png", "color": "#4B3621"},
        {"model": "styles/black_wave_hair.png", "color": "#2C2C2C"},
        
    ],
    "Uzun": [
        {"model": "styles/textured_fringe_black.png", "color": "#000000"},
        {"model": "styles/messy_brown_hair.png", "color": "#8B4513"},
        {"model": "styles/textured_fringe_black.png", "color": "#000000"},
    ],
}

# Görsel servis etme endpoint'i
# API tarafından oluşturulan görselleri kullanıcıya döndürmek.
@app.route('/uploads/<path:filename>')
def serve_uploaded_file(filename):
    return send_from_directory(os.getcwd(), filename)

# Ana API endpoint'i
#Kullanıcıdan bir görsel alır, yüz şeklini analiz eder ve uygun bir saç modeli uygular.
@app.route('/analyze', methods=['POST'])
def analyze_photo():
    if 'file' not in request.files:
        return jsonify({"error": "Dosya yüklenmedi"}), 400
 #Dosya Kaydetme
    file = request.files['file']
    image_path = "uploaded_image.jpg"
    output_path = "styles.png"
    file.save(image_path)
    #Yüz Şekli Analizi
    face_shape, _ = analyze_face(image_path)
    if not face_shape:
        return jsonify({"error": "Yüz tespit edilemedi"}), 400
       #Saç Modeli Seçimi
    recommendations = HAIR_RECOMMENDATIONS.get(face_shape, [{"model": "styles/default.png", "color": "#FFFFFF"}])
    recommendation = random.choice(recommendations)

    hair_model_path = recommendation["model"]
#overlay_hair fonksiyonu ile seçilen saç modeli, yüz görseline bindiril
    result_image = overlay_hair(image_path, hair_model_path)
    if result_image is None:
        return jsonify({"error": "Saç modeli bindirme başarısız"}), 500

    cv2.imwrite(output_path, result_image)

    return jsonify({
        "face_shape": face_shape,
        "hair_model": recommendation["model"],
        "hair_color": recommendation["color"],
        "output_image": "http://127.0.0.1:5000/uploads/styles.png"
    })

if __name__ == '__main__':
    app.run(debug=True)
