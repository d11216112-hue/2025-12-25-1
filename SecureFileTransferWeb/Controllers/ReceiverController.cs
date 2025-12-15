using Microsoft.AspNetCore.Mvc;
using SecureFileTransferWeb.Services;

namespace SecureFileTransferWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiverController : ControllerBase
    {
        private readonly ILogger<ReceiverController> _logger;
        private static ReceiverService? _receiverService;

        public ReceiverController(ILogger<ReceiverController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// 啟動接收服務
        /// </summary>
        [HttpPost("start")]
        public IActionResult Start([FromBody] StartRequest request)
        {
            try
            {
                if (_receiverService != null && _receiverService.IsListening)
                {
                    return BadRequest("接收服務已在執行中");
                }

                _logger.LogInformation($"啟動接收服務，Port: {request.Port}, 目錄: {request.SaveDirectory}");

                _receiverService = new ReceiverService();
                _receiverService.Start(request.Port, request.SaveDirectory);

                return Ok(new
                {
                    success = true,
                    message = "接收服務已啟動",
                    port = request.Port,
                    saveDirectory = request.SaveDirectory
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "啟動接收服務時發生錯誤");
                return StatusCode(500, $"啟動失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 停止接收服務
        /// </summary>
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            try
            {
                if (_receiverService == null || !_receiverService.IsListening)
                {
                    return BadRequest("接收服務未執行");
                }

                _logger.LogInformation("停止接收服務");
                _receiverService.Stop();

                return Ok(new
                {
                    success = true,
                    message = "接收服務已停止"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "停止接收服務時發生錯誤");
                return StatusCode(500, $"停止失敗: {ex.Message}");
            }
        }

        /// <summary>
        /// 取得接收狀態
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                if (_receiverService == null)
                {
                    return Ok(new
                    {
                        isListening = false,
                        isReceiving = false,
                        receivedFiles = Array.Empty<object>(),
                        newMessages = Array.Empty<object>()
                    });
                }

                var newMessages = _receiverService.GetNewMessages();

                return Ok(new
                {
                    isListening = _receiverService.IsListening,
                    isReceiving = _receiverService.IsReceiving,
                    receivedFiles = _receiverService.ReceivedFiles,
                    newMessages = newMessages
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "取得狀態時發生錯誤");
                return StatusCode(500, $"取得狀態失敗: {ex.Message}");
            }
        }
    }

    /// <summary>
    /// 啟動請求
    /// </summary>
    public class StartRequest
    {
        public int Port { get; set; } = 8000;
        public string SaveDirectory { get; set; } = "./received";
    }
}
