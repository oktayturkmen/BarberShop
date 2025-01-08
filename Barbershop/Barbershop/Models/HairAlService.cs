using System.Net.Http; 
using System.Text; 
using System.Threading.Tasks; // Asenkron işlemleri yönetmek için gerekli
using Newtonsoft.Json; 

public class HairAIService
{
    private readonly string _apiUrl; // Flask API'nin temel URL'sini saklamak için

    // Sınıf yapıcısı (constructor) - API URL'si parametre olarak alınıyor
    public HairAIService(string apiUrl)
    {
        _apiUrl = apiUrl; // Kullanıcının sağladığı URL, sınıf içindeki değişkene atanıyor
    }

    // Belirtilen resim dosyasını analiz etmek için bir asenkron metot
    public async Task<string> AnalyzePhotoAsync(string filePath)
    {
        using (var httpClient = new HttpClient()) // HTTP istekleri için HttpClient oluşturuluyor
        {
            // Form-data içeriği oluşturuluyor (resim dosyası göndermek için)
            var form = new MultipartFormDataContent();

            // Resim dosyasını byte dizisi olarak oku
            var fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(filePath));
            // Dosya içeriğinin türünü belirt (örneğin, JPEG formatı)
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("image/jpeg");
            // Resim dosyasını form-data içine ekle
            form.Add(fileContent, "file", "uploaded_image.jpg");

            // Flask API'ye POST isteği gönder
            var response = await httpClient.PostAsync(_apiUrl + "/analyze", form);

            // Eğer API çağrısı başarısız olursa, bir hata fırlat
            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException($"API çağrısı başarısız: {response.StatusCode}");
            }

            // API'den dönen JSON yanıtını string olarak al
            var jsonResponse = await response.Content.ReadAsStringAsync();

            // Yanıtı geri döndür
            return jsonResponse;
        }
    }
}
//API URL'si Alınıyor: HairAIService sınıfı, Flask API'nin temel URL'sini parametre olarak alıyor.
//Resim Gönderiliyor: AnalyzePhotoAsync metodu, verilen resim dosyasını POST isteğiyle API'ye gönderiyor. Resim, bir form-data olarak yükleniyor.
//Sonuç Alınıyor: API'den gelen yanıt (JSON formatında), string olarak döndürülüyor. Eğer istek başarısız olursa hata fırlatılıyor.
//Bu sınıf, fotoğraf analizi için bir Flask API ile iletişim kurmayı sağlıyor.