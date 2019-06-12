using SourceAFIS.Simple;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging; 

//'PresentationCore' , 'WindowsBase' , 'System.Xaml'  references has added


namespace ImageVerificationSystem
{
    public class Program
    {
        // Inherit from Fingerprint in order to add Filename field
        [Serializable]
        class MyFingerprint : Fingerprint
        {
            public string Filename;
        }
        static readonly string ImagePath = Path.Combine(Path.Combine("..", ".."), "images");
        static AfisEngine Afis;

        // Inherit from Person in order to add Name field
        [Serializable]
        class MyPerson : Person
        {
            public string Name;
        }

        public static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }

        static MyPerson Enroll(string filename, byte[] byteArray)
        {
            MyFingerprint fp = new MyFingerprint();
            fp.Filename = filename;
            BitmapImage image = Program.ToImage(byteArray);
            fp.AsBitmapSource = image;
            MyPerson person = new MyPerson();
            person.Name = filename; 
            person.Fingerprints.Add(fp);
            Afis.Extract(person);
            return person;
        }

        static void Main(string[] args)
        {
            Afis = new AfisEngine();
            List<MyPerson> database = new List<MyPerson>();
            List<byte[]> ListOfImages = new List<byte[]>();
            string pathString = string.Empty;
            byte[] byteArray;
            int ideal_value = 0;

            pathString = System.IO.Path.Combine(ImagePath, "fingerprint_2_93.tif");
            byteArray = System.IO.File.ReadAllBytes(pathString);
            database.Add(Enroll(Path.Combine(ImagePath, "fingerprint_2_93.tif"), byteArray));

            pathString = System.IO.Path.Combine(ImagePath, "fingerprint_3_83.tif");
            byteArray = System.IO.File.ReadAllBytes(pathString);
            database.Add(Enroll(Path.Combine(ImagePath, "fingerprint_3_83.tif"), byteArray));

            pathString = System.IO.Path.Combine(ImagePath, "probe_2_125.tif");
            byteArray = System.IO.File.ReadAllBytes(pathString);
            MyPerson probe = Enroll(Path.Combine(ImagePath, "probe_2_125.tif"), byteArray);

            Afis.Threshold = 10;
            MyPerson match = Afis.Identify(probe, database).FirstOrDefault() as MyPerson;
            if (match == null)
            {
                //return "No matching person found.";
            }

            float score = Afis.Verify(probe, match);
            string matched_user = match.Name;

            if (ideal_value <= 0)
            {
                ideal_value = 60;
            }

            if (score > ideal_value)
            {
                Console.WriteLine("Matched user: " + matched_user);
                Console.WriteLine("Score: " + score);
            }
            else
            {
                Console.WriteLine("No Matched user");
                Console.WriteLine("Score: " + score);
            }            
            Console.ReadLine();
        }
    }
}
