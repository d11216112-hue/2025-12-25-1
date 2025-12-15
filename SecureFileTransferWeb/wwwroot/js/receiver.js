// 接收端 JavaScript

document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('receiverForm');
    const startBtn = document.getElementById('startBtn');
    const stopBtn = document.getElementById('stopBtn');
    const statusIndicator = document.getElementById('statusIndicator');
    const statusLog = document.getElementById('statusLog');
    const receivedFiles = document.getElementById('receivedFiles');

    let isListening = false;
    let pollInterval = null;

    // 表單提交事件 - 啟動接收
    form.addEventListener('submit', async function(e) {
        e.preventDefault();

        const listenPort = document.getElementById('listenPort').value;
        const saveDirectory = document.getElementById('saveDirectory').value;

        try {
            statusLog.innerHTML = '';
            addStatusMessage('⚙️ 正在啟動接收服務...', 'info');

            const response = await fetch('/api/receiver/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json'
                },
                body: JSON.stringify({
                    port: parseInt(listenPort),
                    saveDirectory: saveDirectory
                })
            });

            if (response.ok) {
                const result = await response.json();
                isListening = true;
                updateUI('listening');
                
                addStatusMessage('✓ 接收服務已啟動！', 'success');
                addStatusMessage(`📡 監聽 Port: ${listenPort}`, 'info');
                addStatusMessage(`💾 儲存目錄: ${saveDirectory}`, 'info');
                addStatusMessage('⏳ 等待發送端連線中...', 'info');

                // 開始輪詢狀態
                startPolling();
            } else {
                const error = await response.text();
                throw new Error(error || '啟動失敗');
            }

        } catch (error) {
            addStatusMessage('❌ 錯誤: ' + error.message, 'error');
            addStatusMessage('💡 請確認 Port 是否已被其他程式佔用', 'warning');
        }
    });

    // 停止接收
    stopBtn.addEventListener('click', async function() {
        try {
            const response = await fetch('/api/receiver/stop', {
                method: 'POST'
            });

            if (response.ok) {
                isListening = false;
                updateUI('idle');
                stopPolling();
                
                addStatusMessage('⏹️ 接收服務已停止', 'warning');
            }
        } catch (error) {
            addStatusMessage('❌ 停止失敗: ' + error.message, 'error');
        }
    });

    // 開始輪詢狀態
    function startPolling() {
        pollInterval = setInterval(async () => {
            try {
                const response = await fetch('/api/receiver/status');
                if (response.ok) {
                    const status = await response.json();
                    
                    if (status.isReceiving) {
                        updateUI('receiving');
                    } else if (status.isListening) {
                        updateUI('listening');
                    }

                    // 更新已接收檔案列表
                    if (status.receivedFiles && status.receivedFiles.length > 0) {
                        updateReceivedFiles(status.receivedFiles);
                    }

                    // 顯示新訊息
                    if (status.newMessages && status.newMessages.length > 0) {
                        status.newMessages.forEach(msg => {
                            addStatusMessage(msg.text, msg.type);
                        });
                    }
                }
            } catch (error) {
                console.error('輪詢狀態失敗:', error);
            }
        }, 1000);
    }

    // 停止輪詢
    function stopPolling() {
        if (pollInterval) {
            clearInterval(pollInterval);
            pollInterval = null;
        }
    }

    // 更新 UI 狀態
    function updateUI(state) {
        statusIndicator.classList.remove('status-idle', 'status-listening', 'status-receiving');
        
        switch(state) {
            case 'idle':
                statusIndicator.classList.add('status-idle');
                statusIndicator.querySelector('.status-text').textContent = '閒置中';
                startBtn.style.display = 'inline-block';
                stopBtn.style.display = 'none';
                break;
            case 'listening':
                statusIndicator.classList.add('status-listening');
                statusIndicator.querySelector('.status-text').textContent = '監聽中';
                startBtn.style.display = 'none';
                stopBtn.style.display = 'inline-block';
                break;
            case 'receiving':
                statusIndicator.classList.add('status-receiving');
                statusIndicator.querySelector('.status-text').textContent = '接收中';
                startBtn.style.display = 'none';
                stopBtn.style.display = 'inline-block';
                break;
        }
    }

    // 更新已接收檔案列表
    function updateReceivedFiles(files) {
        if (files.length === 0) {
            receivedFiles.innerHTML = '<p class="empty-message">尚未接收任何檔案</p>';
            return;
        }

        receivedFiles.innerHTML = '';
        files.forEach(file => {
            const fileItem = document.createElement('div');
            fileItem.className = 'file-item';
            fileItem.innerHTML = `
                <div class="file-item-info">
                    <div class="file-item-name">📄 ${file.name}</div>
                    <div class="file-item-details">
                        大小: ${formatFileSize(file.size)} | 
                        接收時間: ${new Date(file.receivedTime).toLocaleString('zh-TW')}
                    </div>
                </div>
            `;
            receivedFiles.appendChild(fileItem);
        });
    }

    // 輔助函數：格式化檔案大小
    function formatFileSize(bytes) {
        if (bytes === 0) return '0 Bytes';
        const k = 1024;
        const sizes = ['Bytes', 'KB', 'MB', 'GB'];
        const i = Math.floor(Math.log(bytes) / Math.log(k));
        return Math.round(bytes / Math.pow(k, i) * 100) / 100 + ' ' + sizes[i];
    }

    // 輔助函數：新增狀態訊息
    function addStatusMessage(message, type = 'info') {
        const p = document.createElement('p');
        p.textContent = message;
        p.className = 'status-' + type;
        statusLog.appendChild(p);
        statusLog.scrollTop = statusLog.scrollHeight;
    }

    // 頁面卸載時停止服務
    window.addEventListener('beforeunload', function() {
        if (isListening) {
            fetch('/api/receiver/stop', { method: 'POST' });
        }
    });
});
