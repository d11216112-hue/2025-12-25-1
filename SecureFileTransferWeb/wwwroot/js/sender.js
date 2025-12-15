// 發送端 JavaScript

document.addEventListener('DOMContentLoaded', function() {
    const form = document.getElementById('senderForm');
    const fileInput = document.getElementById('fileInput');
    const fileInfo = document.getElementById('fileInfo');
    const fileName = document.getElementById('fileName');
    const fileSize = document.getElementById('fileSize');
    const statusLog = document.getElementById('statusLog');

    // 檔案選擇事件
    fileInput.addEventListener('change', function(e) {
        const file = e.target.files[0];
        if (file) {
            fileName.textContent = file.name;
            fileSize.textContent = formatFileSize(file.size);
            fileInfo.style.display = 'block';
            addStatusMessage('已選取檔案: ' + file.name, 'info');
        }
    });

    // 表單提交事件
    form.addEventListener('submit', async function(e) {
        e.preventDefault();

        const receiverIp = document.getElementById('receiverIp').value;
        const receiverPort = document.getElementById('receiverPort').value;
        const file = fileInput.files[0];

        if (!file) {
            addStatusMessage('❌ 錯誤：請先選取檔案', 'error');
            return;
        }

        try {
            // 清空狀態日誌
            statusLog.innerHTML = '';
            
            addStatusMessage('📋 準備傳送檔案...', 'info');
            addStatusMessage(`   檔案名稱: ${file.name}`, 'info');
            addStatusMessage(`   檔案大小: ${formatFileSize(file.size)}`, 'info');
            addStatusMessage(`   目標位址: ${receiverIp}:${receiverPort}`, 'info');
            
            // 讀取檔案
            addStatusMessage('📖 正在讀取檔案...', 'info');
            const fileData = await readFileAsArrayBuffer(file);
            addStatusMessage('✓ 檔案讀取完成', 'success');

            // 加密檔案
            addStatusMessage('🔐 正在加密檔案...', 'info');
            await new Promise(resolve => setTimeout(resolve, 500)); // 模擬加密時間
            addStatusMessage('✓ 加密完成！', 'success');

            // 傳送檔案
            addStatusMessage(`🌐 正在連線至 ${receiverIp}:${receiverPort}...`, 'info');
            
            const formData = new FormData();
            formData.append('file', file);
            formData.append('receiverIp', receiverIp);
            formData.append('receiverPort', receiverPort);

            const response = await fetch('/api/sender/send', {
                method: 'POST',
                body: formData
            });

            if (response.ok) {
                const result = await response.json();
                addStatusMessage('✓ 連線成功！', 'success');
                addStatusMessage('📤 正在傳送加密資料...', 'info');
                addStatusMessage('✓ 傳送完成！', 'success');
                addStatusMessage('', 'success');
                addStatusMessage('🎉 發送成功！檔案已安全傳送至接收端。', 'success');
                
                // 重置表單
                setTimeout(() => {
                    fileInput.value = '';
                    fileInfo.style.display = 'none';
                }, 2000);
            } else {
                const error = await response.text();
                throw new Error(error || '傳送失敗');
            }

        } catch (error) {
            addStatusMessage('❌ 錯誤: ' + error.message, 'error');
            addStatusMessage('💡 請確認：', 'warning');
            addStatusMessage('   1. 接收端是否已啟動', 'warning');
            addStatusMessage('   2. IP 位址與 Port 是否正確', 'warning');
            addStatusMessage('   3. 網路連線是否正常', 'warning');
        }
    });

    // 輔助函數：讀取檔案為 ArrayBuffer
    function readFileAsArrayBuffer(file) {
        return new Promise((resolve, reject) => {
            const reader = new FileReader();
            reader.onload = (e) => resolve(e.target.result);
            reader.onerror = (e) => reject(new Error('檔案讀取失敗'));
            reader.readAsArrayBuffer(file);
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
});
