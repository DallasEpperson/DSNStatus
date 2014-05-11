using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DSNStatus
{
    public static class DSNPoller
    {
        public static List<Spacecraft> Spacecrafts;
        public static List<Site> Sites;
        public static List<Dish> Dishes;

        public static DSNStatusResult GetStatus(DateTime dateTime)
        {
            var requestPeriod = (Int32) (dateTime.ToUniversalTime().Subtract(new DateTime(1970, 1, 1)).TotalSeconds);

            var dsnConfigXMLURL = @"http://eyes.nasa.gov/dsn/config.xml";
            var dsnStatusXMLURL = @"http://eyes.nasa.gov/dsn/data/dsn.xml?r=" + requestPeriod/5;
            // TODO figure out why it doesn't seem to be respecting request period

            if (Spacecrafts == null || Spacecrafts.Count == 0
                || Sites == null || Sites.Count == 0
                || Dishes == null || Dishes.Count == 0)
            {
                Spacecrafts = new List<Spacecraft>();
                Sites = new List<Site>();
                Dishes = new List<Dish>();
                var dsnConfigXMLDeserializer = new XmlSerializer(typeof (DSNXMLConfigResult));
                var dsnConfigXML = (DSNXMLConfigResult)
                    dsnConfigXMLDeserializer.Deserialize(WebRequest.Create(dsnConfigXMLURL).GetResponse().GetResponseStream());

                foreach (var site in dsnConfigXML.Sites.Site)
                {
                    var newSite = new Site
                        {
                            ID = site.Name,
                            Name = site.FriendlyName,
                            FlagURL = site.Flag
                        };
                    Sites.Add(newSite);

                    foreach (var dish in site.Dishes)
                    {
                        var newDish = new Dish
                            {
                                ID = dish.Name,
                                Location = site.Name,
                                Name = dish.FriendlyName,
                                Type = dish.DishType
                            };
                        Dishes.Add(newDish);
                    }
                }

                foreach (var spacecraft in dsnConfigXML.SpacecraftMap.Spacecrafts)
                {
                    var newSpacecraft = new Spacecraft
                        {
                            ID = spacecraft.Name,
                            ExplorerName = spacecraft.ExplorerName,
                            Name = spacecraft.FriendlyName,
                            Thumbnail = (spacecraft.Thumbnail == "true")
                        };
                    Spacecrafts.Add(newSpacecraft);
                }

                Dishes.Sort((a, b) => a.Name.CompareTo(b.Name));
                Sites.Sort((a, b) => a.Name.CompareTo(b.Name));
                Spacecrafts.Sort((a, b) => a.Name.CompareTo(b.Name));
            }

            var dsnStatusXMLDeserializer = new XmlSerializer(typeof(DSNXMLStatusResult));
            var dsnStatusXML = (DSNXMLStatusResult)
                dsnStatusXMLDeserializer.Deserialize(WebRequest.Create(dsnStatusXMLURL).GetResponse().GetResponseStream());

            // parse through XML as above
            foreach (var station in dsnStatusXML.Stations)
            {
                if (Sites.Any(s => s.ID == station.Name))
                {
                    var updateSite = Sites.First(s => s.ID == station.Name);
                    updateSite.TimezoneOffsetMinutes = String.IsNullOrEmpty(station.TimeZoneOffset) ? 0 : (int.Parse(station.TimeZoneOffset) / 1000) / 60;
                    updateSite.TimeReportedUTC = String.IsNullOrEmpty(station.TimeUTC) ? (new DateTime()) : (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(double.Parse(station.TimeUTC));
                }
            }

            foreach (var dish in dsnStatusXML.Dishes)
            {
                foreach (var target in dish.Targets)
                {
                    if (Spacecrafts.Any(s => s.ID.ToUpper() == target.Name.ToUpper()))
                    {
                        var updateSpacecraft = Spacecrafts.First(s => s.ID.ToUpper() == target.Name.ToUpper());
                        updateSpacecraft.UplegRange = string.IsNullOrEmpty(target.UplegRange) ? 0 : double.Parse(target.UplegRange);
                        updateSpacecraft.DownlegRange = string.IsNullOrEmpty(target.DownlegRange) ? 0 : double.Parse(target.DownlegRange);
                        updateSpacecraft.RTLT = string.IsNullOrEmpty(target.RoundTripLightTime) ? 0 : double.Parse(target.RoundTripLightTime);
                    }
                }
                
                if (Dishes.Any(d => d.ID == dish.Name))
                {
                    var updateDish = Dishes.First(d => d.ID == dish.Name);
                    updateDish.AzimuthAngle = string.IsNullOrEmpty(dish.AzimuthAngle)? 0 : decimal.Parse(dish.AzimuthAngle);
                    updateDish.ElevationAngle = string.IsNullOrEmpty(dish.ElevationAngle) ? 0 : decimal.Parse(dish.ElevationAngle);
                    updateDish.WindSpeed = string.IsNullOrEmpty(dish.WindSpeed) ? 0 : decimal.Parse(dish.WindSpeed);
                    updateDish.IsMSPA = (dish.IsMSPA == "true");
                    updateDish.IsArray = (dish.IsArray == "true");
                    updateDish.IsDDOR = (dish.IsDDOR == "true");
                    updateDish.Created = string.IsNullOrEmpty(dish.Created) ? new DateTime() : DateTime.Parse(dish.Created);
                    updateDish.Updated = string.IsNullOrEmpty(dish.Updated) ? new DateTime() : DateTime.Parse(dish.Updated);
                    updateDish.Signals = new List<Signal>();

                    foreach (var downSignal in dish.DownSignals)
                    {
                        if (!string.IsNullOrEmpty(downSignal.SignalType) && downSignal.SignalType != "none")
                        {
                            var newSignal = new Signal();
                            newSignal.Direction = "Down";
                            newSignal.Type = downSignal.SignalType;
                            newSignal.TypeDebug = downSignal.SignalTypeDebug;
                            newSignal.DataRate = string.IsNullOrEmpty(downSignal.DataRate) ? 0 : double.Parse(downSignal.DataRate);
                            newSignal.Frequency = string.IsNullOrEmpty(downSignal.Frequency) ? 0 : double.Parse(downSignal.Frequency);
                            newSignal.Power = string.IsNullOrEmpty(downSignal.Power) ? 0 : double.Parse(downSignal.Power);
                            newSignal.Spacecraft = Spacecrafts.FirstOrDefault(s => s.ID.ToUpper() == downSignal.Spacecraft.ToUpper());
                            updateDish.Signals.Add(newSignal);
                        }
                    }

                    foreach (var upSignal in dish.UpSignals)
                    {
                        if (!string.IsNullOrEmpty(upSignal.SignalType) && upSignal.SignalType != "none")
                        {
                            var newSignal = new Signal();
                            newSignal.Direction = "Up";
                            newSignal.Type = upSignal.SignalType;
                            newSignal.TypeDebug = upSignal.SignalTypeDebug;
                            newSignal.DataRate = string.IsNullOrEmpty(upSignal.DataRate) ? 0 : double.Parse(upSignal.DataRate);
                            newSignal.Frequency = string.IsNullOrEmpty(upSignal.Frequency) ? 0 : double.Parse(upSignal.Frequency);
                            newSignal.Power = string.IsNullOrEmpty(upSignal.Power) ? 0 : double.Parse(upSignal.Power);
                            newSignal.Spacecraft = Spacecrafts.FirstOrDefault(s => s.ID.ToUpper() == upSignal.Spacecraft.ToUpper());
                            updateDish.Signals.Add(newSignal);
                        }
                    }

                    updateDish.Targets = new List<Spacecraft>();
                    if (updateDish.Signals.Count > 0)
                    {
                        foreach (var target in dish.Targets)
                        {
                            if (Spacecrafts.Any(s => s.ID.ToUpper() == target.Name.ToUpper()))
                            {
                                updateDish.Targets.Add(Spacecrafts.First(s => s.ID.ToUpper() == target.Name.ToUpper()));
                            }
                        }
                    }
                }
            }

            var Updated = string.IsNullOrEmpty(dsnStatusXML.Timestamp) ? (new DateTime()) : (new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)).AddMilliseconds(double.Parse(dsnStatusXML.Timestamp));

            return new DSNStatusResult
            {
                Spacecrafts = Spacecrafts,
                Sites = Sites,
                Dishes = Dishes,
                LastUpdated = Updated
            };
        }

        public class Spacecraft
        {
            public string ID;
            public string ExplorerName;
            public string Name;
            public bool Thumbnail;
            public double UplegRange;
            public double DownlegRange;
            public double RTLT;

            public override string ToString()
            {
                return ID + " - " + Name;
            }
        }

        public class Site
        {
            public string ID;
            public string Name;
            public string FlagURL;
            public DateTime TimeReportedUTC;
            public int TimezoneOffsetMinutes;

            public override string ToString()
            {
                return Name;
            }
        }

        public class Dish
        {
            public string ID;
            public string Name;
            public string Type;
            public string Location;
            public decimal AzimuthAngle;
            public decimal ElevationAngle;
            public decimal WindSpeed;
            /// <summary>
            /// Multiple Spacecraft Per Aperture
            /// </summary>
            public bool IsMSPA;
            public bool IsArray; // TODO What does this even mean?

            /// <summary>
            /// Delta-Differenced One-Way Range
            /// </summary>
            public bool IsDDOR;

            public DateTime Created;
            public DateTime Updated;
            // TODO ^ What do these signify?

            public List<Spacecraft> Targets;
            public List<Signal> Signals;

            public override string ToString()
            {
                return Name;
            }
        }

        public class Signal
        {
            public string Direction;
            public string Type;
            public string TypeDebug;
            public double DataRate;
            public double Frequency;
            public double Power;
            public Spacecraft Spacecraft;
        }

        public class DSNStatusResult
        {
            public List<Spacecraft> Spacecrafts;
            public List<Site> Sites;
            public List<Dish> Dishes;
            public DateTime LastUpdated;
        }
    }


    # region XML Structure
    [XmlRoot("dsn")]
    public class DSNXMLStatusResult
    {
        [XmlElement("station")]
        public List<DSNXMLStation> Stations;
        
        [XmlElement("dish")]
        public List<DSNXMLDish> Dishes;

        [XmlElement("timestamp")] 
        public string Timestamp; // TODO convert to useful format
    }

    [XmlRoot("config")]
    public class DSNXMLConfigResult
    {
        [XmlElement("sites")]
        public DSNXMLSites Sites;

        [XmlElement("spacecraftMap")]
        public DSNXMLSpacecraftMap SpacecraftMap; 
    }

    public class DSNXMLSites
    {
        [XmlElement("site")]
        public List<DSNXMLSite> Site; 
    }

    public class DSNXMLSite
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("friendlyName")]
        public string FriendlyName;
        
        [XmlAttribute("flag")]
        public string Flag;

        [XmlElement("dish")]
        public List<DSNXMLDish> Dishes;
    }

    public class DSNXMLSpacecraftMap
    {
        [XmlElement("spacecraft")]
        public List<DSNXMLSpacecraft> Spacecrafts; 
    }

    public class DSNXMLSpacecraft
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("explorerName")]
        public string ExplorerName;

        [XmlAttribute("friendlyName")]
        public string FriendlyName;

        [XmlAttribute("thumbnail")]
        public string Thumbnail;
    }

    public class DSNXMLStation
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("friendlyName")]
        public string FriendlyName;

        [XmlAttribute("timeUTC")]
        public string TimeUTC; // TODO convert to useful format

        [XmlAttribute("timeZoneOffset")]
        public string TimeZoneOffset; // TODO convert to useful format
    }

    public class DSNXMLDish
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("azimuthAngle")]
        public string AzimuthAngle;

        [XmlAttribute("elevationAngle")]
        public string ElevationAngle;

        [XmlAttribute("windSpeed")]
        public string WindSpeed;

        /// <summary>
        /// Multiple Spacecraft Per Aperture
        /// </summary>
        [XmlAttribute("isMSPA")]
        public string IsMSPA;

        [XmlAttribute("isArray")]
        public string IsArray; // TODO what does this mean

        /// <summary>
        /// Delta-Differenced One-Way Range
        /// </summary>
        [XmlAttribute("isDDOR")]
        public string IsDDOR;

        [XmlAttribute("created")]
        public string Created; // TODO what does this mean

        [XmlAttribute("updated")]
        public string Updated;

        [XmlElement("downSignal")]
        public List<DSNXMLDownSignal> DownSignals;

        [XmlElement("upSignal")]
        public List<DSNXMLUpSignal> UpSignals; 

        [XmlElement("target")]
        public List<DSNXMLTarget> Targets; 

        // For Config
        [XmlAttribute("friendlyName")]
        public string FriendlyName;

        [XmlAttribute("type")]
        public string DishType;
    }

    public class DSNXMLSignal
    {
        [XmlAttribute("signalType")]
        public string SignalType;

        [XmlAttribute("signalTypeDebug")]
        public string SignalTypeDebug;

        [XmlAttribute("dataRate")]
        public string DataRate;

        [XmlAttribute("frequency")]
        public string Frequency;

        [XmlAttribute("power")]
        public string Power;

        [XmlAttribute("spacecraft")]
        public string Spacecraft;

        [XmlAttribute("spacecraftId")]
        public string SpacecraftID;
    }

    public class DSNXMLDownSignal : DSNXMLSignal
    {
        public string Direction = "Down";
    }

    public class DSNXMLUpSignal : DSNXMLSignal
    {
        public string Direction = "Up";
    }

    public class DSNXMLTarget
    {
        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("id")]
        public string ID;

        [XmlAttribute("uplegRange")]
        public string UplegRange; // todo whatever holds 1.5617004311978E10

        [XmlAttribute("downlegRange")]
        public string DownlegRange; // todo whatever holds 1.5617004311978E10 double

        [XmlAttribute("rtlt")]
        public string RoundTripLightTime;
        // TODO whatever holds 104182.545343
    }
# endregion

}