using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Support.V4.App;
using Android.Views;
using Android.Widget;

namespace TestRun
{
    [Service]
    public class ForegroundMethod : Service
    {
        public const int SERVICE_ID = 10000;
        
        public override IBinder OnBind(Intent intent)
        {
            throw new NotImplementedException();
        }
        
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            var notification = new NotificationCompat.Builder(this, "1100")
                .SetContentTitle("Location")
                .SetContentText("You can't close me")
                .SetSmallIcon(P6Test.Resource.Drawable.IMG_0485)
                //.SetLargeIcon()
                .SetOngoing(true)
                .Build();
            NotificationManager notificationManager = GetSystemService(Context.NotificationService) as NotificationManager;
            const int notificationId = 0;
            notificationManager.Notify(notificationId, notification);

            StartForeground(SERVICE_ID, notification);

            return StartCommandResult.Sticky;
        }
    }
}