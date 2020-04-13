using System;
using System.Text;
using Xamarin.Essentials;
using Android.Content;
using Android.Gms.Location;
using Android.Widget;


namespace TestRun
{
    [BroadcastReceiver]
    public class MyLocationService : BroadcastReceiver
    {
        public static string ACTION_PROCESS_LOCATION = "TestRun.UPDATE_LOCATION";
        public override void OnReceive(Context context, Intent intent)
        {

            if(intent != null)
            {
                string action = intent.Action;
                if (action.Equals(ACTION_PROCESS_LOCATION))
                {
                    LocationResult result = LocationResult.ExtractResult(intent);
                    if(result != null)
                    {
                        var location = result.LastLocation;
                        var speedkmh = location.Speed * 3.6;

                        Preferences.Set("newLong", location.Longitude);
                        Preferences.Set("newLat", location.Latitude);

                        //Preferences.Get("lastCalmLatitude", location.Latitude);
                        //Preferences.Get("lastCalmLongitude", location.Longitude);
                        if (Preferences.Get("setFirstCompared", 0) == 0)
                        {
                            Preferences.Set("setFirstCompared", 1);
                            Preferences.Set("lastComparedLatitude", location.Latitude);
                            Preferences.Set("lastComparedLongitude", location.Longitude);
                        }

                        if (Preferences.Get("updateFive", 5) == 0){

                            Console.WriteLine("Hello");

                            if (isPositionChanged(Preferences.Get("newLat", location.Latitude)
                                , Preferences.Get("newLong", location.Longitude)
                                , Preferences.Get("lastComparedLatitude", location.Latitude)
                                , Preferences.Get("lastComparedLongitude", location.Longitude)))
                            {
                                if (Preferences.Get("setCalmPostion", 0) == 0)
                                {
                                    Preferences.Set("setCalmPosition", 1);
                                    Preferences.Get("lastCalmLatitude", Preferences.Get("lastComparedLatitude", location.Latitude));
                                    Preferences.Get("lastCalmLongitude", Preferences.Get("lastComparedLongitude", location.Longitude));
                                }
                                Preferences.Set("isTrip", true);
                                Preferences.Set("lastComparedLatitude", location.Latitude);
                                Preferences.Set("lastComparedLongitude", location.Longitude);
                            }

                            else if (Preferences.Get("isTrip", false))
                            {
                                if (!isPositionChanged(Preferences.Get("newLat", location.Latitude)
                                , Preferences.Get("newLong", location.Longitude)
                                , Preferences.Get("lastCalmLatitude", Preferences.Get("newLat", location.Latitude))
                                , Preferences.Get("lastCalmLongitude", Preferences.Get("newLong", location.Longitude))))
                                {
                                    resetVaribles(location.Latitude, location.Longitude);

                                    Console.WriteLine("Reset");
                                    Toast.MakeText(context, "Trip is deleted", ToastLength.Short).Show();

                                }
                                else
                                {
                                    Console.WriteLine("Trip saved");
                                    //And save the trip.
                                    Toast.MakeText(context, "Trip is saved", ToastLength.Short).Show();
                                    resetVaribles(location.Latitude, location.Longitude);

                                }
                            }
                        }

                        
                        if (Preferences.Get("updateFive", 5) == 0)
                        {
                            Preferences.Set("updateFive", 5);
                        }
                        else { Preferences.Set("updateFive", Preferences.Get("updateFive", 5) - 1); }
                        

                        if (Preferences.Get("isTrip", false))
                        {
                            if (Preferences.Get("isWalking", 0) == 0)
                            {
                                Preferences.Set("updateFive", 5);
                                Preferences.Set("isWalking", 1);
                                Preferences.Set("vehicle", "walk");
                            }

                            if (speedkmh >= 12 && Preferences.Get("isBiking", 0) == 0)
                            {
                                Preferences.Set("vehicle", "bike");
                                Preferences.Set("updateFive", 5);
                                Preferences.Set("isBiking", 1);
                            }

                            if ((speedkmh > 45) && Preferences.Get("isDriving", 0) == 0)
                            {
                                Preferences.Set("vehicle", "car");
                                Preferences.Set("updateFive", 5);
                                Preferences.Set("isDriving", 1);
                            }



                            Console.WriteLine(Preferences.Get("distance", 0.0));
                            var temp = CalculateDistance(Preferences.Get("newLat", location.Latitude)
                             , Preferences.Get("newLong", location.Longitude)
                             , Preferences.Get("oldLat", location.Latitude)
                             , Preferences.Get("oldLong", location.Longitude)) + Preferences.Get("distance", 0.0);

                            Preferences.Set("distance", temp);

                            Console.WriteLine("Dist: " + Preferences.Get("distance", 0.0));

                            Console.WriteLine("Trip Status: " + Preferences.Get("isTrip", false));
                        }
                        
                        try
                        {
                            MainActivity.GetInstance().UpdateUI(Preferences.Get("newLat", location.Latitude).ToString()
                            , Preferences.Get("newLong", location.Longitude).ToString()
                            , Preferences.Get("oldLat", 0.0).ToString()
                            , Preferences.Get("oldLong", 0.0).ToString()
                            , Preferences.Get("lastComparedLatitude", 0.0).ToString()
                            , Preferences.Get("lastComparedLongitude", 0.0).ToString()
                            , Preferences.Get("lastCalmLatitude", 0.0).ToString()
                            , Preferences.Get("lastCalmLongitude", 0.0).ToString()
                            , Preferences.Get("isTrip", false).ToString()
                            , Preferences.Get("updateFive", 0)
                            , Preferences.Get("distance", 0.0).ToString()
                            , Preferences.Get("vehicle", "")
                            , speedkmh
                            );

                        }

                        catch (Exception e)
                        {
                            Toast.MakeText(context, "Something went wrong!", ToastLength.Short).Show();
                            Console.WriteLine(e.StackTrace);
                        }
                        Preferences.Set("oldLong", Preferences.Get("newLong", location.Longitude));
                        Preferences.Set("oldLat", Preferences.Get("newLat", location.Latitude));

                    }
                }
            }
        }

        private double GetDistance()
        {
            return Preferences.Get("distance", 0.0);
        }

        private void resetVaribles(double latitude, double longitude)
        {
            Preferences.Set("updateFive", 5);
            Preferences.Set("isWalking", 0);
            Preferences.Set("isBiking", 0);
            Preferences.Set("isDriving", 0);
            Preferences.Set("isTrip", false);
            Preferences.Set("lastCalmLatitude", latitude);
            Preferences.Set("lastCalmLongitude", longitude);
            Preferences.Set("distance", 0.0);
            Preferences.Set("vehicle", "");
        }

        public bool isPositionChanged(double newLat, double newLong, double oldLat, double oldLong)
        {
            //Test if it work, 
            if (Math.Abs(newLong - oldLong) + Math.Abs(newLat - oldLat) > 0.002)
            {
                return true;
            }
            else {return false;}
        }


        public double CalculateDistance(double newLat, double newLong, double oldLat, double oldLong)
        {
            float[] result = new float[1];
            double distance;
            Android.Locations.Location.DistanceBetween(newLat, newLong, oldLat, oldLong, result);
            distance = result[0];
            return distance;
        }

        public double ConvertMetertoXp(double meter)
        {
            // Assignment
            int currentLevel = 1;
            double totalXp = 0;
            double currentExperience = 0;
            double restXp = 0;
            RpgSystem rpgsystem = new RpgSystem(currentLevel, totalXp, currentExperience, restXp);
            // Metodekald

            double newXp = meter * (Math.Log(rpgsystem.GetCurrentLevel()) / 3);

            return newXp;
        }
    }

    public class RpgSystem
    {
        private int currentLevel;
        private double xpNeededToLevelUp;
        private double xpReceivedFromTrip;
        private double totalXp;
        private double currentExperience;
        private int maxLevel = 50;
        private double restXp;

        public RpgSystem(int currentLevel, double totalXp, double currentExperience, double restXp)
        {
            this.currentLevel = currentLevel;
            this.totalXp = totalXp;
            this.currentExperience = currentExperience;
            this.restXp = restXp;
        }

        public int GetCurrentLevel()
        {
            return currentLevel;
        }

        public double getXpReceivedFromTrip()
        {
            return xpReceivedFromTrip;
        }

        public double getTotalXp()
        {
            return totalXp;
        }

        public double getCurrentExperience()
        {
            return currentExperience;
        }

        public double getRestXp()
        {
            return restXp;
        }

        // Simple method experience needed to level up.
        public double XpToNextLevel()
        {
            xpNeededToLevelUp = 25 * currentLevel * (1 + currentLevel);
            return xpNeededToLevelUp;
        }

        /* Method for leveling up. There are three cases. 
		 * First case: If a person does not receive enough experience to level up his current experience will simply increase. 
		 * Second case: If the person receives exactly enough experience to level up, he will and his current xp will reset.
		 * Third case: If the person receives more xp than what is needed this person will level up. Besides leveling up
		 * the person will also be receiving further experience in his next level. For example if he needs 80 experience
		 * however he receives 100 experience, he will level up to the next level and start with 20 experience. To do this
		 * a supporting method is used called "CalcRestXp".
		 */
        public void LevelUp(int type, int meters) // Testen giver ballade hvis denne er void og ikke int - Pga. Parametrene
        {
            XpMultiplier(type, meters);
            // Total xp for current after a trip
            double XPAfterTrip = currentExperience + xpReceivedFromTrip;
            double xpNeededToLevelUp = XpToNextLevel();

            if (XPAfterTrip < xpNeededToLevelUp)
            {
                currentExperience += xpReceivedFromTrip;
            }
            else if (XPAfterTrip >= xpNeededToLevelUp)
            {
                currentLevel++;
                restXp = CalcRestXp();
                currentExperience = restXp;
            }
            CalcTotalXp();
        }

        // Helper method to calculate the experience that exceeded when leveling up. Used in "LevelUp" method.
        public double CalcRestXp()
        {
            restXp = (currentExperience + xpReceivedFromTrip) - xpNeededToLevelUp;
            return restXp;
        }

        // Simple method to calculate a players total experience as long as they have not reached the max level.
        public void CalcTotalXp()
        {
            if (currentLevel < maxLevel)
            {
                totalXp += xpReceivedFromTrip;
            }
        }


        // Function to ensure that bicycle and walk can get a multiplier on the value 
        public void XpMultiplier(int type, int meters)
        {

            // 1 = Bike, 2 = Walk
            if (type == 1 || type == 2)
            {
                double xpPerMeter = currentLevel / 4;
                double xpReceived = xpPerMeter * meters;

                if (meters >= 5000)
                {
                    xpReceivedFromTrip = xpReceived * 2;
                }
                else if (meters >= 3500)
                {
                    xpReceivedFromTrip = xpReceived * 1.75;
                }
                else if (meters >= 2000)
                {
                    xpReceivedFromTrip = xpReceived * 1.25;
                }
                else
                {
                    xpReceivedFromTrip = xpReceived;
                }
            }
            else if (type == 3)
            {
                double xpPerMeter = currentLevel / 16;
                double xpReceived;
                if (meters <= 2000)
                {
                    xpReceived = xpPerMeter * meters;
                    xpReceivedFromTrip = xpReceived;
                }
                else if (meters > 2000)
                {
                    xpReceived = xpPerMeter * 2000;
                    xpReceivedFromTrip = xpReceived;
                }

            }
            else if (type == 4)
            {

                double xpPerMeter = currentLevel / 4;
                double xpReceived;

                if (meters >= 500)
                {
                    xpReceived = xpPerMeter * meters;
                    xpReceivedFromTrip = xpReceived;
                }
            }
        }

      /*
        public void ConvertMetertoXp(double meter)
        {
            // Metodekald

            xpReceivedFromTrip = meter * (Math.Log(currentLevel) / 3);

        }
        */


        /* Function that is supposed to give xp based on walking distance.
		For each 500 meters that a person for example walks he/she will receive 50 points.
		When this person hits 1000 meters he/she will receive a 1.2 multiplier on the points/xp.
		This will result in 50*2*1.2 = 120 xp instead of the original 100 xp he/she should have received.
		When the person walks 1500 meters he/she will receive the 120 xp and 50 xp on top of it = 170.
		When the person has walked 2000 meters, then instead of receiving 170+50 = 220 xp, the person will be receiving
		220 * 1.75 = 385 experience points etc.
			 */


        // Når trip er færdig så giver vi xp
        /*public int RewardXp(int meters, int type, bool trip)
		{
			int points = 0;
			if (trip == true)
			{
				points = meters;
			}

			return points;
		}*/
    }
}