using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RGeoLib.BuildingSolver
{
    public class Preset
    {
        ////////////////////////
        /// Room Rendering constants
        
        public const double wallThickness = -0.075;
        public const double doorWidth = 0.8;
        public const double doorOffset = 0.2;
        public const double doorClearance = 1.5;

        /// Occupancy
        /// 
        public List<double> occupancy { get; set; }


        /// occupancy preset
        /// 
        public void occupancyStandard()
        {
            List<double> tempList = new List<double>()
            {
            1.2,
            1.5,
            2.0,
            3.5,
            5.0
            };
        }

        // Interpolation Scores
        public static double InterpolateProgramScore(double score, string program, int numBedrooms, bool primary = true)
        {
            double outScore = 0;

            if (program == "bed")
            {
                if (numBedrooms ==0 )
                    outScore = InterpolateScore(score, 15, 25);
                if (numBedrooms > 0 && primary == true)
                    outScore = InterpolateScore(score, 18, 25);
                else
                    outScore = InterpolateScore(score, 15, 25);
            }


            else if (program == "bath")
            {
                if (primary)
                {
                    if (numBedrooms >= 2)
                        outScore = InterpolateScore(score, 30, 31);
                    else
                        outScore = InterpolateScore(score, 25, 31);
                }
                else
                    outScore = InterpolateScore(score, 20, 31);
            }


            else if (program == "living")
            {
                if (numBedrooms == 0)
                    outScore = InterpolateScore(score, 2, 6);
                else if (numBedrooms == 1)
                    outScore = InterpolateScore(score, 3, 6);
                else if (numBedrooms == 2)
                    outScore = InterpolateScore(score, 4, 6);
                else
                    outScore = InterpolateScore(score, 5, 6);
            }

            else if (program == "kitchen")
            {
                if (numBedrooms == 0)
                    outScore = InterpolateScore(score, 4, 8);
                else if (numBedrooms == 1)
                    outScore = InterpolateScore(score, 5, 8);
                else if (numBedrooms == 2)
                    outScore = InterpolateScore(score, 6, 8);
                else
                    outScore = InterpolateScore(score, 7, 8);
            }
            else if (program == "dining")
            {
                if (numBedrooms == 0)
                    outScore = InterpolateScore(score, 2, 6);
                else if (numBedrooms == 1)
                    outScore = InterpolateScore(score, 3, 6);
                else if (numBedrooms == 2)
                    outScore = InterpolateScore(score, 3.5, 6);
                else
                    outScore = InterpolateScore(score, 5, 6);
            }

            return outScore;
        }
        public static double InterpolateScore(double score, double in_min, double in_max)
        {
            // Linear interpolation of scores

            double out_min = 1.0;
            double out_max = 2.0;
            //double in_min = 18, in_max = 25;

            double interpolatedScore = (score - in_min) * (out_max - out_min) / (in_max - in_min) + out_min;

            return interpolatedScore;
        }

        // AREA Multipliers
        public static double areaMultiplierPreset(double area, int numBedrooms, string preset_location = "Default")
        {
            double output = 0;

            if (preset_location == "Default")
            {
                if (numBedrooms == 0)
                {
                    output = areaMultiplierFunction(area, 18, 25, 38, 48);
                }
                else if (numBedrooms == 1)
                {
                    output = areaMultiplierFunction(area, 33, 43, 55, 64);
                }
                else if (numBedrooms == 2)
                {
                    output = areaMultiplierFunction(area, 49, 59, 79, 89);
                }
                else if (numBedrooms == 3)
                {
                    output = areaMultiplierFunction(area, 63, 73, 100, 111);
                }
                else // if (numBedrooms > 3)
                {
                    output = areaMultiplierFunction(area, 73, 83, 115, 200);
                }
            }
            return output;
        }

        public static double areaMultiplierFunction(double inputValue, double range_min, double range_good_min, double range_good_max, double range_max, double defaultValue = 0.5)
        {
            // if value is lower than range_min or greater than range_max, multiplier = 0
            if (inputValue <= range_min || inputValue >= range_max)
            {
                return defaultValue;
            }
            // if value is between range_min and range_good_min, multiplier linearly goes from 0 to 1
            else if (inputValue > range_min && inputValue <= range_good_min)
            {
                return (inputValue - range_min) / (range_good_min - range_min);
            }
            // if value is between range_good_min and range_good_max, multiplier = 1
            else if (inputValue > range_good_min && inputValue <= range_good_max)
            {
                return 1;
            }
            // if value is between range_good_max and range_max, multiplier linearly goes from 1 to 0
            else if (inputValue > range_good_max && inputValue < range_max)
            {
                return 1 - ((inputValue - range_good_max) / (range_max - range_good_max));
            }
            else
            {
                return defaultValue;
            }
        }

        // Layout Loss score

        public static double layoutLoss(double livingArea, double furnitureArea)
        {
            //double areaLoss = 1 - (areaFurniture / ( areaFurniture + areaCirculation));
            double areaLoss = Math.Max(0, (1 - (furnitureArea / (livingArea * 0.8))));

            return areaLoss;
        }
    }
}
