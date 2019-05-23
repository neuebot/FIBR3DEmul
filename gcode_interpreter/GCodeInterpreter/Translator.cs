using System;
using System.Text;
using System.Xml.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace GCodeInterpreter
{
    /// <summary>
    /// Goal of this class is to translate the information from the Parser class
    /// into language that can be transmitted to the robot. 
    /// By default supports 5 axis communication.
    /// 
    /// Commands we need to transmit:
    /// RapidMove // IncRapidMove
    /// LinearMove
    /// ArcMove
    /// Stop Feed
    /// Start Feed
    /// 
    /// Convention
    /// Units: mm and radians
    /// 
    /// Type of message
    /// JSON serialization
    /// [code][string][num ints][ints][num doubles][doubles]
    /// 
    /// </summary>
    /// 

    // Enum used to clarify codes for functions
    // Odd values are reserved for replies
    public enum move_codes
    {
        RapidMove = 1000,
        IncRapidMove = 1002,
        LinearMove = 1020,
        IncLinearMove = 1022,
        ArcMoveCenter = 1040,
        ArcMoveRadius = 1042,
        CircleMove = 1044,
        IncArcMoveCenter = 1046,
        IncArcMoveRadius = 1048,
        IncCircleMove = 1050,
        Dwell = 1080
    }

    public class PackageJSON
    {
        public int keycode;
        public bool clkwise;
        public bool interpol;
        public bool extrude;
        public string text;

        public double aux;

        public double[] pos1;
        public double[] pos2;
        
        public int plane;

        public double[] acc; //3-axis acceleration values
        public double[] vel; //current, target and next velocity
        public int line; //current gcode line

        public PackageJSON()
        {
            keycode = 0;
            clkwise = false;
            interpol = false;
            extrude = false;
            text = "";
            aux = 0.0;
            plane = 0;
            pos1 = new double[5];
            pos2 = new double[5];
            vel = new double[3];
            acc = new double[3];
            line = 0;
        }
    }

    class LineJSON
    {
        public int line;
        public bool collision;

        public LineJSON()
        {
            line = 0;
            collision = false;         
        }
    }

    class Translator
    {
        /// <summary>
        /// RAPID MOVE
        /// 
        /// Movement of each axis should start and stop at the same time. Linear velocity of each axis, with 
        /// at least one of them moving at the highest speed (the one that covers a larger distance), 
        /// while others move at a scaled velocity. 
        /// </summary>
        /// <param name="pos">Final absolute position</param>
        /// <returns></returns>
        public string RapidMove(double[] pos, double[] vel, double[] acc, bool extrude, bool abs_coord, int line)
        {
            PackageJSON package = new PackageJSON();
            package.line = line;
            //Check if sending incremental or absolute coordinates
            if (abs_coord)
            {
                package.keycode = (int)move_codes.RapidMove;
                package.text = "Rapid Move";
            }
            else
            {
                package.keycode = (int)move_codes.IncRapidMove;
                package.text = "Incremental Rapid Move";
            }

            package.pos1 = pos;
            package.vel = vel;
            package.acc = acc;
            package.extrude = extrude;

            string pack_json = JsonConvert.SerializeObject(package);

            return pack_json;
        }


        /// <summary>
        /// LINEAR MOVE
        /// 
        /// Movement is transversed at the specified feed rate (primary velocity mm/min), 
        /// on a straight line to the target position.
        /// The movement of all axis is synchronous, i.e., they stop moving simultaneously.
        /// </summary>
        /// <param name="pos">Final absolute position</param>
        /// <returns></returns>
        public string LinearMove(double[] pos, double[] vel, double[] acc, bool extrude, bool interpolated, bool abs_coord, int line)
        {
            PackageJSON package = new PackageJSON();
            package.line = line;
            //Check if sending incremental or absolute coordinates
            if (abs_coord)
            {
                package.keycode = (int)move_codes.LinearMove;
                package.text = "Linear Move";
            }
            else
            {
                package.keycode = (int)move_codes.IncLinearMove;
                package.text = "Linear Rapid Move";
            }

            package.pos1 = pos;
            package.interpol = interpolated;
            package.vel = vel;
            package.acc = acc;
            package.extrude = extrude;

            string pack_json = JsonConvert.SerializeObject(package);

            return pack_json;
        }

        /// <summary>
        /// ARC MOVE
        /// 
        /// Movement is transversed at the specified feed rate (primary velocity mm/min), 
        /// on a circular movement to the target position. Plane of motion defined as working plane (G17-19).
        /// The movement of all axis is synchronous, i.e., they stop moving simultaneously.
        /// 
        /// Ka start point of circle (current)
        /// Ke end point of circle (X-,Y-,Z-)
        /// Km center point of circle (I-,J-,K-) if not programmed [0,0,0]
        /// R radius (alternative to IJK)
        /// </summary>
        /// <param name="pos">absolute end point</param>
        /// <param name="cen">absolute center point</param>
        /// <param name="clkwise">clockwise / counterclockwise</param>
        /// <returns></returns>
        public string ArcMove(double[] pos, double[] cen, WorkingPlane plane, bool clkwise, double[] vel, double[] acc, bool extrude, 
            bool interpolated, bool abs_coord, int line)
        {
            PackageJSON package = new PackageJSON();
            package.line = line;
            if (abs_coord)
            {
                package.keycode = (int)move_codes.ArcMoveCenter;
                package.text = "Arc Move Center";
            }
            else
            {
                package.keycode = (int)move_codes.IncArcMoveCenter;
                package.text = "Incremental Arc Move Center";
            }

            package.pos1 = pos;
            package.pos2 = cen;
            package.plane = (int)plane;
            package.clkwise = clkwise;
            package.interpol = interpolated;
            package.vel = vel;
            package.acc = acc;
            package.extrude = extrude;

            string pack_json = JsonConvert.SerializeObject(package);

            return pack_json;
        }

        // With R
        public string ArcMove(double[] pos, double radius, WorkingPlane plane, bool clkwise, double[] vel, double[] acc, bool extrude, 
            bool interpolated, bool abs_coord, int line)
        {
            PackageJSON package = new PackageJSON();
            package.line = line;
            if (abs_coord)
            {
                package.keycode = (int)move_codes.ArcMoveRadius;
                package.text = "Arc Move Radius";
            }
            else
            {
                package.keycode = (int)move_codes.IncArcMoveRadius;
                package.text = "Incremental Arc Move Radius";
            }

            package.pos1 = pos;
            package.aux = radius;
            package.plane = (int)plane;
            package.clkwise = clkwise;
            package.interpol = interpolated;
            package.vel = vel;
            package.acc = acc;
            package.extrude = extrude;

            string pack_json = JsonConvert.SerializeObject(package);

            return pack_json;
        }

        // Full circle
        public string ArcMove(double[] cen, WorkingPlane plane, bool clkwise, double[] vel, double[] acc, bool extrude, bool interpolated, bool abs_coord, int line)
        {
            PackageJSON package = new PackageJSON();
            package.line = line;
            if (abs_coord)
            {
                package.keycode = (int)move_codes.CircleMove;
                package.text = "Circle Move";
            }
            else
            {
                package.keycode = (int)move_codes.IncCircleMove;
                package.text = "Incremental Circle Move";
            }
            ////Convert milimeters to meters
            //for (int i = 0; i < 3; ++i)
            //{
            //    cen[i] /= 1000;
            //    vel[i] /= 1000;
            //    acc[i] /= 1000;
            //}

            package.pos2 = cen;
            package.plane = (int)plane;
            package.clkwise = clkwise;
            package.interpol = interpolated;
            package.vel = vel;
            package.acc = acc;
            package.extrude = extrude;

            string pack_json = JsonConvert.SerializeObject(package);

            return pack_json;
        }

        /// <summary>
        /// DWELL
        /// 
        /// Stops motion for a specific time frame (milliseconds). 
        /// </summary>
        /// <param name="time">Defined either at X-coordinate or No_letter-coordinate</param>
        /// <returns></returns>
        public string Dwell(double time, int line)
        {
            PackageJSON package = new PackageJSON();
            package.line = line;
            package.keycode = (int)move_codes.Dwell;
            package.text = "Dwell";
            package.aux = time;

            string pack_json = JsonConvert.SerializeObject(package);

            return pack_json;
        }

        //Auxiliary functions
        /// <summary>
        /// Simplest way would be to Parse the string using JToken.Parse, and also to check if the string 
        /// starts with { or [ and ends with } or ] respectively:
        /// 
        /// The reason to add checks for { or [ etc was based on the fact that JToken.Parse would parse 
        /// the values such as "1234" or "'a string'" as a valid token. The other option could be to use 
        /// both JObject.Parse and JArray.Parse in parsing and see if anyone of them succeeds, but I 
        /// believe checking for {} and [] should be easier. (Thanks @RhinoDevel for pointing it out)
        /// 
        /// https://stackoverflow.com/questions/14977848/how-to-make-sure-that-string-is-valid-json-using-json-net
        /// </summary>
        /// <param name="strInput"></param>
        /// <returns></returns>
        public static bool IsValidJson(string strInput)
        {
            strInput = strInput.Trim();
            if ((strInput.StartsWith("{") && strInput.EndsWith("}")) || //For object
                (strInput.StartsWith("[") && strInput.EndsWith("]"))) //For array
            {
                try
                {
                    var obj = JToken.Parse(strInput);
                    return true;
                }
                catch (JsonReaderException jex)
                {
                    //Exception in parsing json
                    Console.WriteLine(jex.Message);
                    return false;
                }
                catch (Exception ex) //some other exception
                {
                    Console.WriteLine(ex.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        public static string GetUntilOrEmpty(string text, string stopAt = "}")
        {
            if (!String.IsNullOrWhiteSpace(text))
            {
                int charLocation = text.IndexOf(stopAt, StringComparison.Ordinal);

                if (charLocation > 0)
                {
                    return text.Substring(0, charLocation + 1);
                }
            }

            return String.Empty;
        }

        public static Tuple<int, bool> GetLine(string JSON_msg)
        {
            LineJSON lj = JsonConvert.DeserializeObject<LineJSON>(JSON_msg);

            Tuple<int, bool> res = new Tuple<int, bool>(lj.line, lj.collision);

            return res;
        }
    }
}
