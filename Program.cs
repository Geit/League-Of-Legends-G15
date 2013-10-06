using GammaJul.LgLcd;
using System;
using System.Drawing;
using System.IO;
using System.Media;
using System.Reflection;
using System.Threading;

namespace LoLTimer
{

	internal class Program
	{
		private static readonly AutoResetEvent _waitAre = new AutoResetEvent(false);
		private static volatile bool _monoArrived;
		private static volatile bool _mustExit;
		enum Mobs : int
		{
			Baron = 0,
			Dragon,
			Golem_Blue,
			Lizard_Blue,
			Golem_Purple,
			Lizard_Purple
		}
		public static DateTime[] RespawnDateTimes = new DateTime[6];
		public static string[] TagNames = new string[] 
		{ 
			"BaronCountdownText", 
			"DragonCountdownText", 
			"GolemBlueCountdownText", 
			"LizardBlueCountdownText", 
			"GolemPurpleCountdownText", 
			"LizardPurpleCountdownText"
		};
		public static bool[] isMobDead = new bool[6];
		public static int[] RespawnTimes = new int[6] { 
			420,
			360,
			300,
			300,
			300,
			300
		};

		public static LcdGdiPage
			Page_MainPage,
			Page_RespawnBaron,
			Page_RespawnDragon;

		public static SoundPlayer SoundPlayer;
		public static int Current_CampIndex;
		private static LcdDeviceMonochrome monoDevice = null;

		/// <summary>
		/// Entry point of the program.
		/// </summary>
		[MTAThread]
		internal static void Main()
		{
			SoundPlayer = new SoundPlayer();
			// Create a new applet for the monochrome LCD Screen and set it to auto-start.
			LcdApplet applet = new LcdApplet("League Of Legends Timer G15", LcdAppletCapabilities.Monochrome, true);

			// Register to events to know when a device arrives, then connects the applet to the LCD Manager
			applet.DeviceArrival += Applet_DeviceArrival;
			applet.DeviceRemoval += Applet_DeviceRemoval;
			applet.Connect();

			// We are waiting for the handler thread to warn us for device arrival
			_waitAre.WaitOne();
			do
			{

				// A monochrome device was connected: creates a monochrome device or reopens an old one
				if (_monoArrived)
				{
					if (monoDevice == null)
					{
						monoDevice = (LcdDeviceMonochrome)applet.OpenDeviceByType(LcdDeviceType.Monochrome);
						monoDevice.SoftButtonsChanged += MonoDevice_SoftButtonsChanged;
						CreateMonochromeGdiPages(monoDevice);
					}
					else
						monoDevice.ReOpen();
					_monoArrived = false;
				}

				// We are calling DoUpdateAndDraw in this loop.
				// Note that updating and drawing only happens if the objects in a LcdGdiPage are modified.
				// Even if you call this method very quickly, update and draw will only occur at the frame
				// rate specified by LcdPage.DesiredFrameRate, which is 30 by default.
				if (applet.IsEnabled && monoDevice != null && !monoDevice.IsDisposed)
					monoDevice.DoUpdateAndDraw();

				Thread.Sleep(5);
			}
			while (!_mustExit);
		}

		private static LcdGdiText getPageTextElement(LcdGdiPage page, String tag)
		{
			return ((LcdGdiText)page.Children.Find(child => (String)child.Tag == tag));
		}

		/// <summary>
		/// Creates two new LcdGdiPages for a monochrome device.
		/// </summary>
		/// <param name="monoDevice">Device to use for page creation.</param>
		private static void CreateMonochromeGdiPages(LcdDevice monoDevice)
		{
			// Creates first page
			Page_MainPage = new LcdGdiPage(monoDevice)
			{
				Children = 
				{
                    new LcdGdiText 
                    {
                        Text = "Baron: ",

                        Margin = new MarginF(0.0f, 5f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
                    },
                    new LcdGdiText
                    {
                        Text = TimeSpan.FromSeconds(RespawnTimes[(int)Mobs.Baron]).ToString(@"m\:ss"),
                        Tag = TagNames[(int)Mobs.Baron],

                        Margin = new MarginF(28.0f, -2.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif, 16)
                    },
                   new LcdGdiText 
                   {
                        Text = "Dragon: ",

                        Margin = new MarginF(75.0f, 5f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
                    },
                    new LcdGdiText
                    {
                        Text = TimeSpan.FromSeconds(RespawnTimes[(int)Mobs.Dragon]).ToString(@"m\:ss"),
						Tag = TagNames[(int)Mobs.Dragon],

                        Margin = new MarginF(110.0f, -2.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif, 16)   
                    },
                   new LcdGdiText 
                   {
                        Text = "Blue Golem: ",

                        Margin = new MarginF(0.0f, 20.0f, 0.0f, 0.0f),
						Font = new Font(FontFamily.GenericSansSerif,7)

                    },
					new LcdGdiText 
                   {
                        Text = TimeSpan.FromSeconds(RespawnTimes[(int)Mobs.Golem_Blue]).ToString(@"m\:ss"),
                        Tag = TagNames[(int)Mobs.Golem_Blue],

                        Margin = new MarginF(55.0f, 20.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
                    },
                   new LcdGdiText 
                   {
                        Text = "Blue Lizard : ",

                        Margin = new MarginF(0.0f, 30.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
                    },
					new LcdGdiText 
                   {
                        Text = TimeSpan.FromSeconds(RespawnTimes[(int)Mobs.Lizard_Blue]).ToString(@"m\:ss"),
                        Tag = TagNames[(int)Mobs.Lizard_Blue],

                        Margin = new MarginF(55.0f, 30.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
                    },
                   new LcdGdiText 
                   {
                        Text = "Purple Golem: ",
                        HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
                        Margin = new MarginF(75.0f, 20.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
				   },
				   new LcdGdiText 
                   {
                        Text = TimeSpan.FromSeconds(RespawnTimes[(int)Mobs.Golem_Purple]).ToString(@"m\:ss"),
                        Tag = TagNames[(int)Mobs.Golem_Purple],

                        HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
                        Margin = new MarginF(138.0f, 20.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
                    },
                   new LcdGdiText 
                   {
                        Text = "Purple Lizard : ",
                        HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
                        Margin = new MarginF(75.0f, 30.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7),                        
                    },
					new LcdGdiText 
                   {
                        Text = TimeSpan.FromSeconds(RespawnTimes[(int)Mobs.Lizard_Purple]).ToString(@"m\:ss"),
                        Tag = TagNames[(int)Mobs.Lizard_Purple],

                        HorizontalAlignment = LcdGdiHorizontalAlignment.Stretch,
						VerticalAlignment = LcdGdiVerticalAlignment.Stretch,
                        Margin = new MarginF(138.0f, 30.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,7)
                    },
					new LcdGdiLine
					{
						Pen = Pens.Black,
						Tag = "currentSelectionLine",
						StartPoint = new PointF(56.0f, 30.0f),
						EndPoint = new PointF(74.0f, 30.0f)
					}
				}
			};
			Page_MainPage.Updating += Page_Updating;
			Page_MainPage.DesiredFramerate = 10;


			// Adds page to the device's Pages collection (not mandatory, but helps for storing pages),
			// and sets the first page as the current page
			monoDevice.Pages.Add(Page_MainPage);
			monoDevice.CurrentPage = Page_MainPage;

			Page_RespawnDragon = new LcdGdiPage(monoDevice)
			{
				Children =
				{
					new LcdGdiImage
					{
						Image = LoLTimer.Resources.img_dragon,
						Margin = new MarginF(5.0f, 0.0f, 0.0f, 0.0f),
					},
					new LcdGdiText
					{
						Text = "Dragon",
                        Margin = new MarginF(50.0f, -5.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,22)
					},
					new LcdGdiText
					{
						Text = "Has Respawned",
                        Margin = new MarginF(48.0f, 25.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif, 11)
					}
				}
			};
			Page_RespawnBaron = new LcdGdiPage(monoDevice)
			{
				Children =
				{
					new LcdGdiImage
					{
						Image = LoLTimer.Resources.img_baron,
						Margin = new MarginF(5.0f, 6.0f, 0.0f, 0.0f),
					},
					new LcdGdiText
					{
						Text = "Baron",
                        Margin = new MarginF(60.0f, -5.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif,22)
					},
					new LcdGdiText
					{
						Text = "Has Respawned",
                        Margin = new MarginF(48.0f, 25.0f, 0.0f, 0.0f),
                        Font = new Font(FontFamily.GenericSansSerif, 11)
					}
				}
			};

			monoDevice.Pages.Add(Page_RespawnDragon);
			monoDevice.Pages.Add(Page_RespawnBaron);
			
		}

		/// <summary>
		/// This event handler is called whenever the soft buttons are pressed or released.
		/// </summary>
		private static void MonoDevice_SoftButtonsChanged(object sender, LcdSoftButtonsEventArgs e)
		{
			LcdDevice device = (LcdDevice)sender;
			Console.WriteLine(e.SoftButtons);

			// First button (remember that buttons start at index 0) is pressed, switch to page one
			if ((e.SoftButtons & LcdSoftButtons.Button0) == LcdSoftButtons.Button0)
			{
				RespawnDateTimes[(int) Mobs.Baron] = DateTime.Now.AddSeconds(RespawnTimes[(int)Mobs.Baron]);
				isMobDead[(int)Mobs.Baron] = true;
			}
			// Second button is pressed, switch to page two
			if ((e.SoftButtons & LcdSoftButtons.Button1) == LcdSoftButtons.Button1)
			{
				RespawnDateTimes[(int)Mobs.Dragon] = DateTime.Now.AddSeconds(RespawnTimes[(int)Mobs.Dragon]);
				isMobDead[(int)Mobs.Dragon] = true;
			}
			// Third button is pressed, do a garbage collection (for testing purpose only!)
			if ((e.SoftButtons & LcdSoftButtons.Button2) == LcdSoftButtons.Button2)
			{
				var mobIndex = 2 + Current_CampIndex;
				RespawnDateTimes[mobIndex] = DateTime.Now.AddSeconds(RespawnTimes[mobIndex]);
				isMobDead[mobIndex] = true;
			}

			// Fourth button is pressed, exit
			if ((e.SoftButtons & LcdSoftButtons.Button3) == LcdSoftButtons.Button3)
			{
				Current_CampIndex = (Current_CampIndex + 1) % 4;


				SoundPlayer.Stream = LoLTimer.Resources.snd_scroll;
				SoundPlayer.Play();

				var line = ((LcdGdiLine)Page_MainPage.Children.Find(child => (String)child.Tag == "currentSelectionLine"));
				var mobIndex = 2 + Current_CampIndex;
				var textElement = getPageTextElement(Page_MainPage, TagNames[mobIndex]);

				line.StartPoint = new PointF(textElement.AbsolutePosition.X + 2, textElement.AbsolutePosition.Y + 10);
				line.EndPoint = new PointF((textElement.AbsolutePosition + textElement.FinalSize).X + 1, (textElement.AbsolutePosition + textElement.FinalSize).Y);
			}
			else if (e.SoftButtons > 0)
			{
				SoundPlayer.Stream = LoLTimer.Resources.snd_select;
				SoundPlayer.Play();
			}
		}

		/// <summary>
		/// This event handler is called before the page starts its update.
		/// </summary>
		private static void Page_Updating(object sender, UpdateEventArgs e)
		{
			LcdGdiPage page = (LcdGdiPage) sender;
			foreach (int MobIndex in Mobs.GetValues(typeof(Mobs)))
			{
				if (RespawnDateTimes[MobIndex] >= DateTime.Now)
				{
					TimeSpan ts = RespawnDateTimes[MobIndex].Subtract(DateTime.Now);
					getPageTextElement(Page_MainPage, TagNames[MobIndex]).Text = ts.ToString(@"m\:ss");
				}
				else if (RespawnDateTimes[MobIndex] < DateTime.Now && isMobDead[MobIndex])
				{
					isMobDead[MobIndex] = false;
					SoundPlayer.Stream = LoLTimer.Resources.snd_respawn;
					SoundPlayer.Play();

					getPageTextElement(Page_MainPage, TagNames[MobIndex]).Text = TimeSpan.FromSeconds(RespawnTimes[MobIndex]).ToString(@"m\:ss");

					if (MobIndex == (int)Mobs.Baron)
					{
						monoDevice.CurrentPage = Page_RespawnBaron;
						monoDevice.DoUpdateAndDraw();
						Thread.Sleep(3000);
						monoDevice.CurrentPage = Page_MainPage;
					} else if (MobIndex == (int)Mobs.Dragon)
					{
						monoDevice.CurrentPage = Page_RespawnDragon;
						monoDevice.DoUpdateAndDraw();
						Thread.Sleep(3000);
						monoDevice.CurrentPage = Page_MainPage;
					}
				}
			}

		}

		/// <summary>
		/// This event handler will be called whenever a new device of a given type arrives in the system.
		/// This is where you should open the device where you want to show the applet.
		/// Take special care for thread-safety as the SDK calls this handler in another thread.
		/// </summary>
		private static void Applet_DeviceArrival(object sender, LcdDeviceTypeEventArgs e)
		{
			switch (e.DeviceType)
			{

				// A monochrome device (G13/G15/Z10) was connected
				case LcdDeviceType.Monochrome:
					_monoArrived = true;
					break;

			}
			_waitAre.Set();
		}

		/// <summary>
		/// This event handler will be called whenever every device of a given type are disconnected from the system.
		/// You should stop using the device here.
		/// </summary>
		private static void Applet_DeviceRemoval(object sender, LcdDeviceTypeEventArgs e)
		{
		}

	}

}
