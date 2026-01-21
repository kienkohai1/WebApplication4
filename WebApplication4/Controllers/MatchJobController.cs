using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;
using TimViec.Model;

public class MatchJobController : Controller {
    public async Task<IActionResult> MatchJob(IFormFile cvFile)
    {
        if (cvFile == null) return View();

        // 1. Giả lập danh sách công việc (Mock Data) dựa trên Model Job của bạn
        var mockJobs = new List<Job>
    {
        new Job { Id = 1, Title = "Lập trình viên ASP.NET", Description = "Yêu cầu C#, SQL Server, MVC.", Status = "Đã duyệt" },
        new Job { Id = 2, Title = "Data Analyst", Description = "Sử dụng Python, SQL, SPSS để phân tích dữ liệu.", Status = "Đã duyệt" },
        new Job { Id = 3, Title = "Quản lý Sân tập Bắn cung", Description = "Quản lý nhân sự và vận hành sân tập The Hunter.", Status = "Đã duyệt" }
    };

        // 2. Gọi AI Service (Python)
        using (var client = new HttpClient())
        {
            using (var content = new MultipartFormDataContent())
            {
                // Gửi file CV
                var fileStream = cvFile.OpenReadStream();
                var streamContent = new StreamContent(fileStream);
                content.Add(streamContent, "file", cvFile.FileName);

                // Gửi dữ liệu Job giả lập dưới dạng JSON
                var jobsJson = JsonConvert.SerializeObject(mockJobs);
                content.Add(new StringContent(jobsJson, Encoding.UTF8, "application/json"), "jobs_data");

                // Địa chỉ API Python (FastAPI)
                var response = await client.PostAsync("http://127.0.0.1:8000/api/match", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseData = await response.Content.ReadAsStringAsync();
                    var results = JsonConvert.DeserializeObject<List<AIResult>>(responseData);
                    return View("MatchResults", results); // Trả về View kết quả
                }
            }
        }
        return View("Error");
    }
}
