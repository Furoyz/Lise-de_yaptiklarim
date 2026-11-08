using Microsoft.EntityFrameworkCore;
using MinibusTakip.Data;
using WebPush;

namespace MinibusTakip.Services
{
    public class PushNotificationService
    {
        private readonly ApplicationDbContext _db;
        private readonly string _publicKey;
        private readonly string _privateKey;
        private readonly string _subject;
        private readonly ILogger<PushNotificationService> _logger;

        public PushNotificationService(ApplicationDbContext db, IConfiguration config, ILogger<PushNotificationService> logger)
        {
            _db = db;
            _publicKey = config["VapidKeys:PublicKey"]!;
            _privateKey = config["VapidKeys:PrivateKey"]!;
            _subject = config["VapidKeys:Subject"]!;
            _logger = logger;
        }

        public async Task SendToUsersAsync(List<string> userIds, string title, string body, string url = "/Yolcu/Panelim")
        {
            if (userIds == null || !userIds.Any()) return;

            var subs = await _db.UserPushSubscriptions.Where(s => userIds.Contains(s.UserId)).ToListAsync();
            var webPushClient = new WebPushClient();
            var vapidDetails = new VapidDetails(_subject, _publicKey, _privateKey);
            var payload = System.Text.Json.JsonSerializer.Serialize(new { title, body, url });

            foreach (var sub in subs)
            {
                try
                {
                    var pushSub = new PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);
                    await webPushClient.SendNotificationAsync(pushSub, payload, vapidDetails);
                }
                catch (WebPushException ex)
                {
                    if (ex.StatusCode == System.Net.HttpStatusCode.Gone)
                    {
                        // Cihaz artık yok, veritabanından sil
                        _db.UserPushSubscriptions.Remove(sub);
                    }
                    else
                    {
                        _logger.LogError(ex, "Push gönderilemedi");
                    }
                }
            }
            await _db.SaveChangesAsync();
        }
    }
}