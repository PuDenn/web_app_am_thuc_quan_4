// ╔══════════════════════════════════════════════════╗
// ║  CẤU HÌNH IP — CHỈ SỬA FILE NÀY TRƯỚC KHI DEMO ║
// ╚══════════════════════════════════════════════════╝
//
// Bước 1: Gõ ipconfig trong CMD để lấy IP LAN
// Bước 2: Thay IP bên dưới thành IP máy bạn đang dùng
// Bước 3: Lưu file → không cần rebuild gì cả!

var CONFIG = {
  // ← ĐỔI IP NÀY thành IP LAN máy đang chạy API + Web
  SERVER_IP: '192.168.1.5',

  // Tự tính từ SERVER_IP — không cần sửa
  get API_URL() { return 'http://' + this.SERVER_IP + ':5000'; },
  get WEB_URL() { return 'http://' + this.SERVER_IP + ':3000'; },
};
