using Android.App;
using Android.OS;
using Android.Support.V7.App;
using Android.Runtime;
using Android.Widget;
using System;
using Xamarin.Essentials;
using Plugin.Geolocator;
using System.Threading.Tasks;
using Android.Gms.Location;
using Android.Locations;
using Com.Karumi.Dexter;
using Android;
using Com.Karumi.Dexter.Listener.Single;
using Com.Karumi.Dexter.Listener;
using Android.Support.V4.App;
using Android.Content;
using Android.Content.PM;
using Context = Android.Content.Context;

namespace TestRun
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation, ScreenOrientation = ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, Android.Gms.Location.ILocationListener, IPermissionListener
    {
        private int i = 0;
        private static int rangeDif = 1;
        private Bundle bundle;
        private Xamarin.Essentials.Location oldLocation = null;
        private Xamarin.Essentials.Location newLocation = null;
        private bool runGPS = false;
        FusedLocationProviderClient fusedLocationProviderClient;
        LocationRequest locationRequest;

        static MainActivity Instance;

        public static MainActivity GetInstance()
        {
            return Instance;
        }

        protected override void OnStart()
        {
            base.OnStart();
            Toast.MakeText(this, "Welcome back!", ToastLength.Short).Show();
            i = Preferences.Get("i", 0);
        }
        protected override void OnStop()
        {
            base.OnStop();
            Toast.MakeText(this, "You leaving me?", ToastLength.Short).Show();
            Preferences.Set("i", i);

        }

        protected override void OnPause()
        {
            base.OnPause();
            Toast.MakeText(this, "i'm updating", ToastLength.Short).Show();
            Preferences.Set("i", i);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Toast.MakeText(this, "How could you?", ToastLength.Short).Show();

        }



        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(P6Test.Resource.Layout.TestLocation);
            Plugin.CurrentActivity.CrossCurrentActivity.Current.Init(this, bundle);

            Instance = this;

            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            Dexter.WithActivity(this)
                .WithPermission(Manifest.Permission.AccessFineLocation)
                .WithListener(this)
                .Check();

            CreateNotificationChannel();
            Intent StartServiceIntent = new Intent(this, typeof(ForegroundMethod));
            StartServiceIntent.SetAction("Service.action.START_SERVICE");
            
            StartForegroundService(StartServiceIntent);


            /*FindViewById<Button>(P6Test.Resource.Id.LocationBtn).Click += (o, e) =>
            {
                getLocation();
            };

            FindViewById<Button>(P6Test.Resource.Id.resetButton).Click += (o, e) =>
            {
                i = 0;
            }; */
        }
        public void OnLocationChanged(Android.Locations.Location location)
        {
            getLocation();
        }

        /*private LocationManager GetSystemService(object locationService)
        {
            throw new NotImplementedException();
        }*/

        public void CreateNotificationChannel()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                //If Android version under O no need for channels
                return;
            }

            Java.Lang.ICharSequence name = new Java.Lang.String("Channel");
            var channel = new NotificationChannel("1100", name, default);

            var notificationManager = (NotificationManager) GetSystemService(Context.NotificationService);
            notificationManager.CreateNotificationChannel(channel);
        }
        private async Task GetLastLocation()
        {
            Android.Locations.Location location1 = await fusedLocationProviderClient.GetLastLocationAsync();
            //if (location1 != null)
            //{
            TextView txtLocation1 = FindViewById<TextView>(P6Test.Resource.Id.oldLocation);
            TextView txtLocation2 = FindViewById<TextView>(P6Test.Resource.Id.NewLocation);
            TextView speed = FindViewById<TextView>(P6Test.Resource.Id.speed);
            TextView updates = FindViewById<TextView>(P6Test.Resource.Id.updates);

            var position = location1;

            var test = CrossGeolocator.Current;
            test.DesiredAccuracy = 20;

            var position1 = test.GetPositionAsync();


            if (i == 0)
            {
                oldLocation = new Xamarin.Essentials.Location(position.Latitude, position.Longitude);
                newLocation = new Xamarin.Essentials.Location(position.Latitude, position.Longitude);
                txtLocation1.Text = position.Latitude.ToString("#.###") + " : " + position.Longitude.ToString("#.###");
                i = 1;
                updates.Text = i.ToString();
            }
            else
            {
                oldLocation = newLocation;
                newLocation = new Xamarin.Essentials.Location(position.Latitude, position.Longitude);

                if (txtLocation2.Text.Equals(""))
                {
                    txtLocation1.Text = txtLocation1.Text;

                }
                else { txtLocation1.Text = txtLocation2.Text; }

                txtLocation2.Text = position.Latitude.ToString("#.###") + " : " + position.Longitude.ToString("#.###");
                //distance.Text = Location.CalculateDistance(oldLocation, newLocation, DistanceUnits.Kilometers).ToString() + " km";
                updates.Text = i.ToString();
                var speedkmh = position.Speed * 3.6;
                speed.Text = speedkmh.ToString("#.####") + " km/h";
            }
            i++;
            //}
        }

        private async void getLocation()
        {
            await GetLastLocation();
        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        //If user won't allow location to the app
        public void OnPermissionDenied(PermissionDeniedResponse p0)
        {
            Toast.MakeText(this, "You must accept this permission", ToastLength.Short).Show();
        }

        public void OnPermissionGranted(PermissionGrantedResponse p0)
        {
            UpdateLocation();
        }

        private void UpdateLocation()
        {
            BuildLocationRequest();
            fusedLocationProviderClient = LocationServices.GetFusedLocationProviderClient(this);
            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.AccessFineLocation) != Android.Content.PM.Permission.Granted)
                return;
            fusedLocationProviderClient.RequestLocationUpdates(locationRequest, GetPendingIntent());
        }

        private PendingIntent GetPendingIntent()
        {
            Intent intent = new Intent(this, typeof(MyLocationService));
            intent.SetAction(MyLocationService.ACTION_PROCESS_LOCATION);
            return PendingIntent.GetBroadcast(this, 0, intent, PendingIntentFlags.UpdateCurrent);
        }

        private void BuildLocationRequest()
        {
            locationRequest = new LocationRequest();
            locationRequest.SetPriority(LocationRequest.PriorityHighAccuracy);
            locationRequest.SetInterval(5000);
            locationRequest.SetFastestInterval(3000);
            locationRequest.SetSmallestDisplacement(10f);

        }

        public void OnPermissionRationaleShouldBeShown(PermissionRequest p0, IPermissionToken p1)
        {
            throw new NotImplementedException();
        }

        public void UpdateTextView(string text, double speedkmh)
        {
            TextView txtLocation1 = FindViewById<TextView>(P6Test.Resource.Id.oldLocation);
            TextView txtLocation2 = FindViewById<TextView>(P6Test.Resource.Id.NewLocation);
            TextView speed = FindViewById<TextView>(P6Test.Resource.Id.speed);
            TextView updates = FindViewById<TextView>(P6Test.Resource.Id.updates);
            RunOnUiThread(() =>
            {
                if (i == 0)
                {
                    txtLocation1.Text = text;
                    txtLocation2.Text = text;
                    i++;
                }
                else
                {
                    txtLocation1.Text = txtLocation2.Text;
                    txtLocation2.Text = text;
                    i++;
                }
                speed.Text = speedkmh.ToString("#.##") + " km/h";
                updates.Text = i.ToString();
                Preferences.Set("i", i);

            });
        }
        public void UpdateUI(string NewLatitude, string NewLongitude, string OldLatitude, string OldLongitude, string ComparedLatitude, string ComparedLongitude
            , string CalmLatitude, string CalmLongitude, string Trip, int updateValue, string Distance, string Vehicle, double Speed)
        {
            TextView txtNLat = FindViewById<TextView>(P6Test.Resource.Id.Latitude1);
            TextView txtNLong = FindViewById<TextView>(P6Test.Resource.Id.Longitude1);
            TextView txtOLat = FindViewById<TextView>(P6Test.Resource.Id.Latitude2);
            TextView txtOLong = FindViewById<TextView>(P6Test.Resource.Id.Longitude2);
            TextView txtComLat = FindViewById<TextView>(P6Test.Resource.Id.Latitude3);
            TextView txtComLong = FindViewById<TextView>(P6Test.Resource.Id.Longitude3);
            TextView txtCalmLat = FindViewById<TextView>(P6Test.Resource.Id.Latitude4);
            TextView txtCalmLong = FindViewById<TextView>(P6Test.Resource.Id.Longitude4);

            TextView txtTrip = FindViewById<TextView>(P6Test.Resource.Id.Trip);
            TextView txtUpdateValue = FindViewById<TextView>(P6Test.Resource.Id.UpdateValue);
            TextView txtDistance = FindViewById<TextView>(P6Test.Resource.Id.Distance);
            TextView txtVehicle = FindViewById<TextView>(P6Test.Resource.Id.Vehicle);
            TextView txtSpeed = FindViewById<TextView>(P6Test.Resource.Id.Speed);

            

            RunOnUiThread(() =>
            {
                txtNLat.Text = NewLatitude;
                txtNLong.Text = NewLongitude;
                txtOLat.Text = OldLatitude;
                txtOLong.Text = OldLongitude;
                txtComLat.Text = ComparedLatitude;
                txtComLong.Text = ComparedLongitude;
                txtCalmLat.Text = CalmLatitude;
                txtCalmLong.Text = CalmLongitude;

                txtTrip.Text = Trip;
                txtUpdateValue.Text = updateValue.ToString();
                txtDistance.Text = Distance;
                txtVehicle.Text = Vehicle;
                txtSpeed.Text = (Speed.ToString("#.#") + " km/h");
            });
        }



        protected void saveState()
        {

        }

        protected void loadState()
        {

        }
    }
}