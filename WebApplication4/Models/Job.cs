using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TimViec.Model{ 
    public class Job
    {
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        public string Company { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public string Description { get; set; }

        // Cập nhật trạng thái mặc định
        public string Status { get; set; } = "Chờ duyệt"; // Thay "Mới tạo" bằng "Chờ duyệt"

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public DateTime? SavedDate { get; set; }
    }
}