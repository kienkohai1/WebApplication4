from fastapi import FastAPI, UploadFile, File, Form
from sentence_transformers import SentenceTransformer, util
import fitz  # PyMuPDF
import re
import json
import uvicorn

app = FastAPI()

# 1. Load Model (Dùng model LongContext bạn đã chọn)
model = SentenceTransformer('dangvantuan/vietnamese-embedding-LongContext', trust_remote_code=True)

def extract_text_from_pdf(stream):
    doc = fitz.open(stream=stream, filetype="pdf")
    text = "".join([page.get_text() for page in doc])
    return text

def analyze_cv(cv_text):
    # Giữ nguyên logic trích xuất kỹ năng bằng Regex từ file app.py của bạn
    skills_keywords = ["Python", "SQL", "C#", "ASP.NET", "Entity Framework", "SPSS", "Marketing", "Giao tiếp"]
    skills = set(re.findall(r'\b(?:' + '|'.join(re.escape(k) for k in skills_keywords) + r')\b', cv_text, re.IGNORECASE))
    return {"skills": list(skills)}

@app.post("/api/match")
async def match_cv(
    file: UploadFile = File(...), 
    jobs_data: str = Form(...) # Nhận danh sách jobs giả lập từ ASP.NET
):
    # Đọc file CV từ buffer
    cv_bytes = await file.read()
    cv_text = extract_text_from_pdf(cv_bytes)
    cv_analysis = analyze_cv(cv_text)
    
    # Giải mã danh sách Job giả lập được gửi từ ASP.NET
    job_database = json.loads(jobs_data)
    
    # Tính toán Embedding
    job_descs = [j['Description'] for j in job_database]
    job_embeddings = model.encode(job_descs, convert_to_tensor=True)
    cv_embedding = model.encode(cv_text, convert_to_tensor=True)
    
    # Tính toán độ tương đồng (Cosine Similarity)
    scores = util.cos_sim(cv_embedding, job_embeddings)[0]
    
    results = []
    for i, score in enumerate(scores):
        results.append({
            "JobId": job_database[i].get('Id'),
            "Title": job_database[i]['Title'],
            "Score": round(float(score), 4),
            "MatchedSkills": list(set(cv_analysis["skills"]))
        })
    
    # Sắp xếp kết quả từ cao xuống thấp
    return sorted(results, key=lambda x: x['Score'], reverse=True)

if __name__ == "__main__":
    uvicorn.run(app, host="127.0.0.1", port=8000)