#!/bin/bash

# 電子公文安全傳輸系統 - 端對端測試腳本
# End-to-End Test Script for Secure Document Transfer System

echo "=========================================="
echo "電子公文安全傳輸系統 - 自動化測試"
echo "Secure Document Transfer System - E2E Test"
echo "=========================================="
echo ""

# 設置變數
PORT=8000
BASE_DIR="$(pwd)"
RECEIVER_DIR="$BASE_DIR/received_files"
TEST_FILE_1="$BASE_DIR/test_files/test_document.txt"
TEST_FILE_2="$BASE_DIR/test_files/test_image.jpg"

# 清理之前的測試
echo "1. 清理之前的測試資料..."
rm -rf "$RECEIVER_DIR"
mkdir -p "$RECEIVER_DIR"

# 建置專案
echo ""
echo "2. 建置專案..."
dotnet build SecureFileTransfer.sln > /dev/null 2>&1
if [ $? -eq 0 ]; then
    echo "   ✓ 建置成功"
else
    echo "   ✗ 建置失敗"
    exit 1
fi

# 啟動接收端
echo ""
echo "3. 啟動接收端服務..."
cd SecureFileReceiver
echo -e "\n$RECEIVER_DIR" | dotnet run > receiver.log 2>&1 &
RECEIVER_PID=$!
cd ..
sleep 3

if ps -p $RECEIVER_PID > /dev/null; then
    echo "   ✓ 接收端已啟動 (PID: $RECEIVER_PID)"
else
    echo "   ✗ 接收端啟動失敗"
    exit 1
fi

# 測試 1: 傳送文字檔
echo ""
echo "4. 測試 1: 傳送文字檔案..."
cd SecureFileSender
echo -e "\n\n$TEST_FILE_1" | dotnet run > /dev/null 2>&1
cd ..

# 檢查檔案是否接收
sleep 2
RECEIVED_FILE_1=$(ls $RECEIVER_DIR/recv_*_test_document.txt 2>/dev/null | head -1)
if [ -f "$RECEIVED_FILE_1" ]; then
    # 比對檔案內容
    if diff -q "$TEST_FILE_1" "$RECEIVED_FILE_1" > /dev/null; then
        echo "   ✓ 文字檔案傳輸成功且內容正確"
    else
        echo "   ✗ 文字檔案內容不匹配"
        kill $RECEIVER_PID 2>/dev/null
        exit 1
    fi
else
    echo "   ✗ 未收到文字檔案"
    kill $RECEIVER_PID 2>/dev/null
    exit 1
fi

# 測試 2: 傳送圖片檔
echo ""
echo "5. 測試 2: 傳送圖片檔案..."
cd SecureFileSender
echo -e "\n\n$TEST_FILE_2" | dotnet run > /dev/null 2>&1
cd ..

# 檢查檔案是否接收
sleep 2
RECEIVED_FILE_2=$(ls $RECEIVER_DIR/recv_*_test_image.jpg 2>/dev/null | head -1)
if [ -f "$RECEIVED_FILE_2" ]; then
    # 比對檔案內容
    if diff -q "$TEST_FILE_2" "$RECEIVED_FILE_2" > /dev/null; then
        echo "   ✓ 圖片檔案傳輸成功且內容正確"
    else
        echo "   ✗ 圖片檔案內容不匹配"
        kill $RECEIVER_PID 2>/dev/null
        exit 1
    fi
else
    echo "   ✗ 未收到圖片檔案"
    kill $RECEIVER_PID 2>/dev/null
    exit 1
fi

# 停止接收端
echo ""
echo "6. 停止接收端服務..."
kill $RECEIVER_PID 2>/dev/null
wait $RECEIVER_PID 2>/dev/null
echo "   ✓ 接收端已停止"

# 顯示結果
echo ""
echo "=========================================="
echo "測試完成！所有測試通過 ✓"
echo "Test completed! All tests passed ✓"
echo "=========================================="
echo ""
echo "接收的檔案："
ls -lh $RECEIVER_DIR/
echo ""
echo "原始檔案大小："
ls -lh $TEST_FILE_1 $TEST_FILE_2
echo ""
