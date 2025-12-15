using Microsoft.AspNetCore.Mvc;
using SecureFileTransferWeb.Services;

namespace SecureFileTransferWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SenderController : ControllerBase
    {
        private readonly ILogger<SenderController> _logger;

        public SenderController(ILogger<SenderController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 發送加密檔案
        /// </summary>
        [HttpPost("send")]
        public async Task<IActionResult> SendFile([FromForm] IFormFile file, [FromForm] string receiverIp, [FromForm] int receiverPort)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("請選取檔案");
                }

                _logger.LogInformation($"準備發送檔案: {file.FileName} ({file.Length} bytes) 至 {receiverIp}:{receiverPort}");

                // 讀取檔案內容
                byte[] fileData;
                using (var memoryStream = new MemoryStream())
                {
                    await file.CopyToAsync(memoryStream);
                    fileData = memoryStream.ToArray();
                }

                // 加密檔案
                _logger.LogInformation("正在加密檔案...");
                byte[] encryptedData = AesEncryption.Encrypt(fileData);
                _logger.LogInformation($"加密完成，加密後大小: {encryptedData.Length} bytes");

                // 傳送檔案
                _logger.LogInformation($"正在連線至 {receiverIp}:{receiverPort}...");
                await TcpTransferService.SendEncryptedFileAsync(receiverIp, receiverPort, file.FileName, encryptedData);
                _logger.LogInformation("檔案傳送成功！");

                return Ok(new
                {
                    success = true,
                    message = "檔案傳送成功",
                    fileName = file.FileName,
                    originalSize = fileData.Length,
                    encryptedSize = encryptedData.Length
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "發送檔案時發生錯誤");
                return StatusCode(500, $"發送失敗: {ex.Message}");
            }
        }
    }
}
