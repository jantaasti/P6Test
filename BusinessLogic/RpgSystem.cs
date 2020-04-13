using System;

namespace BusinessLogic
{


	public class RpgSystem
	{
		private int currentLevel;
		private double xpNeededToLevelUp;
		private double xpReceivedFromTrip;
		private double totalXp;
		private double currentExperience;
		private int maxLevel = 50;
		private double restXp;

		public RpgSystem(int currentLevel, int totalXp, int currentExperience, int restXp)
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

		public void ConvertMetertoXp(double meter)
		{
			// Metodekald

			xpReceivedFromTrip = meter * (Math.Log(currentLevel) / 3); 

		}



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
