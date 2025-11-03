using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using System.Collections.Concurrent;
using System.IO.Compression;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure file upload limits
builder.Services.Configure<FormOptions>(options =>
{
    options.MultipartBodyLengthLimit = 100 * 1024 * 1024; // 100 MB
    options.ValueLengthLimit = int.MaxValue;
    options.MultipartHeadersLengthLimit = int.MaxValue;
});

builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 100 * 1024 * 1024; // 100 MB
});

// Add services
builder.Services.AddSingleton<FileStorageService>();
builder.Services.AddSingleton<IContentTypeProvider, FileExtensionContentTypeProvider>();

var app = builder.Build();

// Ensure upload directory exists
var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
Directory.CreateDirectory(uploadPath);

app.MapGet("/", () => Results.Content("""
    <!DOCTYPE html>
    <html>
    <head>
        <title>File Upload & Download Demo</title>
        <style>
            body { font-family: Arial, sans-serif; max-width: 1000px; margin: 50px auto; padding: 20px; }
            .section { margin: 30px 0; padding: 20px; border: 1px solid #ddd; border-radius: 5px; }
            .section h2 { margin-top: 0; color: #0066cc; }
            button { padding: 10px 20px; margin: 5px; cursor: pointer; background: #0066cc; color: white; border: none; border-radius: 4px; }
            button:hover { background: #0052a3; }
            input[type="file"] { margin: 10px 0; }
            .file-list { margin-top: 20px; }
            .file-item { padding: 10px; margin: 5px 0; background: #f9f9f9; border-radius: 4px; display: flex; justify-content: space-between; align-items: center; }
            .progress { width: 100%; height: 20px; background: #f0f0f0; border-radius: 4px; overflow: hidden; margin: 10px 0; }
            .progress-bar { height: 100%; background: #0066cc; transition: width 0.3s; }
            .result { margin-top: 10px; padding: 10px; border-radius: 4px; }
            .success { background: #d4edda; color: #155724; }
            .error { background: #f8d7da; color: #721c24; }
        </style>
    </head>
    <body>
        <h1>File Upload & Download Demo</h1>

        <div class="section">
            <h2>1. Single File Upload</h2>
            <input type="file" id="singleFile" />
            <button onclick="uploadSingle()">Upload File</button>
            <div class="progress" id="singleProgress" style="display:none;">
                <div class="progress-bar" id="singleProgressBar"></div>
            </div>
            <div id="singleResult"></div>
        </div>

        <div class="section">
            <h2>2. Multiple Files Upload</h2>
            <input type="file" id="multipleFiles" multiple />
            <button onclick="uploadMultiple()">Upload Files</button>
            <div class="progress" id="multiProgress" style="display:none;">
                <div class="progress-bar" id="multiProgressBar"></div>
            </div>
            <div id="multiResult"></div>
        </div>

        <div class="section">
            <h2>3. Large File Upload (Chunked)</h2>
            <input type="file" id="largeFile" />
            <button onclick="uploadChunked()">Upload in Chunks</button>
            <div class="progress" id="chunkProgress" style="display:none;">
                <div class="progress-bar" id="chunkProgressBar"></div>
            </div>
            <div id="chunkResult"></div>
        </div>

        <div class="section">
            <h2>4. Uploaded Files</h2>
            <button onclick="loadFiles()">Refresh File List</button>
            <div class="file-list" id="fileList"></div>
        </div>

        <script>
            async function uploadSingle() {
                const fileInput = document.getElementById('singleFile');
                const file = fileInput.files[0];
                if (!file) {
                    alert('Please select a file');
                    return;
                }

                const formData = new FormData();
                formData.append('file', file);

                const progressDiv = document.getElementById('singleProgress');
                const progressBar = document.getElementById('singleProgressBar');
                progressDiv.style.display = 'block';

                try {
                    const xhr = new XMLHttpRequest();
                    
                    xhr.upload.addEventListener('progress', (e) => {
                        if (e.lengthComputable) {
                            const percent = (e.loaded / e.total) * 100;
                            progressBar.style.width = percent + '%';
                        }
                    });

                    xhr.addEventListener('load', () => {
                        if (xhr.status === 200) {
                            const data = JSON.parse(xhr.responseText);
                            document.getElementById('singleResult').innerHTML = 
                                '<div class="result success">File uploaded: ' + data.fileName + ' (' + data.size + ' bytes)</div>';
                            loadFiles();
                        } else {
                            document.getElementById('singleResult').innerHTML = 
                                '<div class="result error">Upload failed: ' + xhr.statusText + '</div>';
                        }
                        progressDiv.style.display = 'none';
                    });

                    xhr.open('POST', '/api/upload/single');
                    xhr.send(formData);
                } catch (error) {
                    document.getElementById('singleResult').innerHTML = 
                        '<div class="result error">Error: ' + error.message + '</div>';
                    progressDiv.style.display = 'none';
                }
            }

            async function uploadMultiple() {
                const fileInput = document.getElementById('multipleFiles');
                const files = fileInput.files;
                if (files.length === 0) {
                    alert('Please select files');
                    return;
                }

                const formData = new FormData();
                for (let file of files) {
                    formData.append('files', file);
                }

                const progressDiv = document.getElementById('multiProgress');
                const progressBar = document.getElementById('multiProgressBar');
                progressDiv.style.display = 'block';

                try {
                    const xhr = new XMLHttpRequest();
                    
                    xhr.upload.addEventListener('progress', (e) => {
                        if (e.lengthComputable) {
                            const percent = (e.loaded / e.total) * 100;
                            progressBar.style.width = percent + '%';
                        }
                    });

                    xhr.addEventListener('load', () => {
                        if (xhr.status === 200) {
                            const data = JSON.parse(xhr.responseText);
                            document.getElementById('multiResult').innerHTML = 
                                '<div class="result success">Uploaded ' + data.count + ' files</div>';
                            loadFiles();
                        } else {
                            document.getElementById('multiResult').innerHTML = 
                                '<div class="result error">Upload failed</div>';
                        }
                        progressDiv.style.display = 'none';
                    });

                    xhr.open('POST', '/api/upload/multiple');
                    xhr.send(formData);
                } catch (error) {
                    document.getElementById('multiResult').innerHTML = 
                        '<div class="result error">Error: ' + error.message + '</div>';
                    progressDiv.style.display = 'none';
                }
            }

            async function uploadChunked() {
                const fileInput = document.getElementById('largeFile');
                const file = fileInput.files[0];
                if (!file) {
                    alert('Please select a file');
                    return;
                }

                const chunkSize = 1024 * 1024; // 1 MB chunks
                const chunks = Math.ceil(file.size / chunkSize);
                const fileName = file.name;

                const progressDiv = document.getElementById('chunkProgress');
                const progressBar = document.getElementById('chunkProgressBar');
                progressDiv.style.display = 'block';

                try {
                    for (let i = 0; i < chunks; i++) {
                        const start = i * chunkSize;
                        const end = Math.min(start + chunkSize, file.size);
                        const chunk = file.slice(start, end);

                        const formData = new FormData();
                        formData.append('chunk', chunk);
                        formData.append('fileName', fileName);
                        formData.append('chunkIndex', i.toString());
                        formData.append('totalChunks', chunks.toString());

                        const response = await fetch('/api/upload/chunked', {
                            method: 'POST',
                            body: formData
                        });

                        if (!response.ok) {
                            throw new Error('Chunk upload failed');
                        }

                        const percent = ((i + 1) / chunks) * 100;
                        progressBar.style.width = percent + '%';
                    }

                    document.getElementById('chunkResult').innerHTML = 
                        '<div class="result success">File uploaded successfully in ' + chunks + ' chunks</div>';
                    loadFiles();
                } catch (error) {
                    document.getElementById('chunkResult').innerHTML = 
                        '<div class="result error">Error: ' + error.message + '</div>';
                } finally {
                    progressDiv.style.display = 'none';
                }
            }

            async function loadFiles() {
                try {
                    const response = await fetch('/api/files');
                    const files = await response.json();
                    
                    const fileList = document.getElementById('fileList');
                    if (files.length === 0) {
                        fileList.innerHTML = '<p>No files uploaded yet</p>';
                        return;
                    }

                    fileList.innerHTML = files.map(file => `
                        <div class="file-item">
                            <div>
                                <strong>${file.name}</strong><br>
                                Size: ${formatBytes(file.size)} | Uploaded: ${new Date(file.uploadedAt).toLocaleString()}
                            </div>
                            <div>
                                <button onclick="downloadFile('${file.id}', '${file.name}')">Download</button>
                                <button onclick="deleteFile('${file.id}')" style="background: #dc3545;">Delete</button>
                            </div>
                        </div>
                    `).join('');
                } catch (error) {
                    console.error('Error loading files:', error);
                }
            }

            async function downloadFile(id, name) {
                window.location.href = '/api/download/' + id;
            }

            async function deleteFile(id) {
                if (!confirm('Delete this file?')) return;
                
                try {
                    const response = await fetch('/api/files/' + id, { method: 'DELETE' });
                    if (response.ok) {
                        loadFiles();
                    }
                } catch (error) {
                    alert('Error deleting file');
                }
            }

            function formatBytes(bytes) {
                if (bytes === 0) return '0 Bytes';
                const k = 1024;
                const sizes = ['Bytes', 'KB', 'MB', 'GB'];
                const i = Math.floor(Math.log(bytes) / Math.log(k));
                return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
            }

            // Load files on page load
            loadFiles();
        </script>
    </body>
    </html>
    """, "text/html"));

// Single file upload
app.MapPost("/api/upload/single", async (IFormFile file, FileStorageService storage) =>
{
    if (file == null || file.Length == 0)
        return Results.BadRequest(new { error = "No file provided" });

    var fileInfo = await storage.SaveFileAsync(file);
    return Results.Ok(fileInfo);
});

// Multiple files upload
app.MapPost("/api/upload/multiple", async (IFormFileCollection files, FileStorageService storage) =>
{
    if (files == null || files.Count == 0)
        return Results.BadRequest(new { error = "No files provided" });

    var uploadedFiles = new List<object>();
    foreach (var file in files)
    {
        var fileInfo = await storage.SaveFileAsync(file);
        uploadedFiles.Add(fileInfo);
    }

    return Results.Ok(new { count = uploadedFiles.Count, files = uploadedFiles });
});

// Chunked upload
app.MapPost("/api/upload/chunked", async (
    IFormFile chunk,
    [FromForm] string fileName,
    [FromForm] int chunkIndex,
    [FromForm] int totalChunks,
    FileStorageService storage) =>
{
    await storage.SaveChunkAsync(chunk, fileName, chunkIndex, totalChunks);
    
    if (chunkIndex == totalChunks - 1)
    {
        await storage.MergeChunksAsync(fileName, totalChunks);
    }

    return Results.Ok(new { chunkIndex, totalChunks });
});

// List files
app.MapGet("/api/files", (FileStorageService storage) =>
{
    return Results.Ok(storage.GetAllFiles());
});

// Download file
app.MapGet("/api/download/{id}", (string id, FileStorageService storage, IContentTypeProvider contentTypeProvider) =>
{
    var fileInfo = storage.GetFile(id);
    if (fileInfo == null)
        return Results.NotFound();

    var filePath = Path.Combine("uploads", fileInfo.Id);
    if (!File.Exists(filePath))
        return Results.NotFound();

    contentTypeProvider.TryGetContentType(fileInfo.Name, out var contentType);
    
    return Results.File(filePath, contentType ?? "application/octet-stream", fileInfo.Name);
});

// Download file as stream (for large files)
app.MapGet("/api/download/stream/{id}", async (string id, FileStorageService storage, HttpContext context) =>
{
    var fileInfo = storage.GetFile(id);
    if (fileInfo == null)
        return Results.NotFound();

    var filePath = Path.Combine("uploads", fileInfo.Id);
    if (!File.Exists(filePath))
        return Results.NotFound();

    context.Response.ContentType = "application/octet-stream";
    context.Response.Headers.Append("Content-Disposition", $"attachment; filename=\"{fileInfo.Name}\"");
    
    await using var fileStream = File.OpenRead(filePath);
    await fileStream.CopyToAsync(context.Response.Body);
    
    return Results.Empty;
});

// Delete file
app.MapDelete("/api/files/{id}", (string id, FileStorageService storage) =>
{
    var success = storage.DeleteFile(id);
    return success ? Results.Ok() : Results.NotFound();
});

app.Run();

// File storage service
public class FileStorageService
{
    private readonly ConcurrentDictionary<string, FileMetadata> _files = new();
    private readonly string _uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
    private readonly string _chunksPath = Path.Combine(Directory.GetCurrentDirectory(), "chunks");

    public FileStorageService()
    {
        Directory.CreateDirectory(_uploadPath);
        Directory.CreateDirectory(_chunksPath);
    }

    public async Task<FileMetadata> SaveFileAsync(IFormFile file)
    {
        var id = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_uploadPath, id);

        await using var stream = File.Create(filePath);
        await file.CopyToAsync(stream);

        var metadata = new FileMetadata
        {
            Id = id,
            Name = file.FileName,
            Size = file.Length,
            ContentType = file.ContentType,
            UploadedAt = DateTime.UtcNow
        };

        _files[id] = metadata;
        return metadata;
    }

    public async Task SaveChunkAsync(IFormFile chunk, string fileName, int chunkIndex, int totalChunks)
    {
        var chunkDir = Path.Combine(_chunksPath, fileName);
        Directory.CreateDirectory(chunkDir);

        var chunkPath = Path.Combine(chunkDir, $"chunk_{chunkIndex}");
        await using var stream = File.Create(chunkPath);
        await chunk.CopyToAsync(stream);
    }

    public async Task MergeChunksAsync(string fileName, int totalChunks)
    {
        var id = Guid.NewGuid().ToString();
        var filePath = Path.Combine(_uploadPath, id);
        var chunkDir = Path.Combine(_chunksPath, fileName);

        await using var outputStream = File.Create(filePath);
        for (int i = 0; i < totalChunks; i++)
        {
            var chunkPath = Path.Combine(chunkDir, $"chunk_{i}");
            await using var chunkStream = File.OpenRead(chunkPath);
            await chunkStream.CopyToAsync(outputStream);
        }

        // Clean up chunks
        Directory.Delete(chunkDir, true);

        var metadata = new FileMetadata
        {
            Id = id,
            Name = fileName,
            Size = new FileInfo(filePath).Length,
            ContentType = "application/octet-stream",
            UploadedAt = DateTime.UtcNow
        };

        _files[id] = metadata;
    }

    public FileMetadata? GetFile(string id)
    {
        _files.TryGetValue(id, out var file);
        return file;
    }

    public IEnumerable<FileMetadata> GetAllFiles()
    {
        return _files.Values.OrderByDescending(f => f.UploadedAt);
    }

    public bool DeleteFile(string id)
    {
        if (!_files.TryRemove(id, out _))
            return false;

        var filePath = Path.Combine(_uploadPath, id);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        return true;
    }
}

public class FileMetadata
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public long Size { get; set; }
    public string ContentType { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; }
}

