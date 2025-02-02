﻿using MatterHackers.Agg;
using MatterHackers.Agg.ImageProcessing;
using MatterHackers.Agg.PlatformAbstract;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.PrinterCommunication;
using MatterHackers.MatterControl.PluginSystem;
using System;
using System.IO;

namespace MatterHackers.MatterControl.ConfigurationPage
{
	public class CloudSettingsWidget : SettingsViewBase
	{
		private DisableableWidget notificationSettingsContainer;
        private DisableableWidget cloudSyncContainer;

		private Button configureNotificationSettingsButton;

        private LinkButtonFactory linkButtonFactory = new LinkButtonFactory();


		public CloudSettingsWidget()
			: base(LocalizedString.Get("Cloud Settings"))
		{
			mainContainer.AddChild(new HorizontalLine(separatorLineColor));

			notificationSettingsContainer = new DisableableWidget();            
			notificationSettingsContainer.AddChild(GetNotificationControls());
			mainContainer.AddChild(notificationSettingsContainer);
            mainContainer.AddChild(new HorizontalLine(separatorLineColor));
            cloudSyncContainer = new DisableableWidget();
            cloudSyncContainer.AddChild(GetCloudSyncDashboardControls());
            mainContainer.AddChild(cloudSyncContainer);

			AddChild(mainContainer);


			AddHandlers();
		}

		private void SetDisplayAttributes()
		{

			this.Margin = new BorderDouble(2, 4, 2, 0);
			this.textImageButtonFactory.normalFillColor = RGBA_Bytes.White;
			this.textImageButtonFactory.disabledFillColor = RGBA_Bytes.White;

			this.textImageButtonFactory.FixedHeight = TallButtonHeight;
			this.textImageButtonFactory.fontSize = 11;

			this.textImageButtonFactory.disabledTextColor = RGBA_Bytes.DarkGray;
			this.textImageButtonFactory.hoverTextColor = ActiveTheme.Instance.PrimaryTextColor;
			this.textImageButtonFactory.normalTextColor = RGBA_Bytes.Black;
			this.textImageButtonFactory.pressedTextColor = ActiveTheme.Instance.PrimaryTextColor;

			this.linkButtonFactory.fontSize = 11;
		}

		public static Action enableCloudMonitorFunction = null;

		private void enableCloudMonitor_Click(object sender, EventArgs mouseEvent)
		{
			if (enableCloudMonitorFunction != null)
			{
				UiThread.RunOnIdle(enableCloudMonitorFunction);
			}
		}

		private FlowLayoutWidget GetCloudSyncDashboardControls()
		{
			FlowLayoutWidget cloudSyncContainer = new FlowLayoutWidget();
			cloudSyncContainer.HAnchor |= HAnchor.ParentLeftRight;
			cloudSyncContainer.VAnchor |= Agg.UI.VAnchor.ParentCenter;
			cloudSyncContainer.Margin = new BorderDouble(0, 0, 0, 0);
			cloudSyncContainer.Padding = new BorderDouble(0);

			Agg.Image.ImageBuffer cloudMonitorImage = StaticData.Instance.LoadIcon(Path.Combine("PrintStatusControls", "cloud-24x24.png"));
			if (!ActiveTheme.Instance.IsDarkTheme)
			{
				InvertLightness.DoInvertLightness(cloudMonitorImage);
			}

            ImageWidget cloudSyncIcon = new ImageWidget(cloudMonitorImage);
            cloudSyncIcon.Margin = new BorderDouble(right: 6, bottom: 6);

            TextWidget cloudSyncLabel = new TextWidget(LocalizedString.Get("Cloud Sync"));
            cloudSyncLabel.AutoExpandBoundsToText = true;
            cloudSyncLabel.TextColor = ActiveTheme.Instance.PrimaryTextColor;
            cloudSyncLabel.VAnchor = VAnchor.ParentCenter;

            linkButtonFactory.fontSize = 10;
            Button cloudSyncGoLink = linkButtonFactory.Generate("GO TO DASHBOARD");
            cloudSyncGoLink.ToolTipText = "Open cloud sync dashboard in web browser";
            cloudSyncGoLink.Click += new EventHandler(cloudSyncGoButton_Click);

            cloudSyncContainer.AddChild(cloudSyncIcon);
            cloudSyncContainer.AddChild(cloudSyncLabel);
            cloudSyncContainer.AddChild(new HorizontalSpacer());
            cloudSyncContainer.AddChild(cloudSyncGoLink);

			return cloudSyncContainer;
		}
        

		private TextWidget notificationSettingsLabel;

		private FlowLayoutWidget GetNotificationControls()
		{
			FlowLayoutWidget notificationSettingsContainer = new FlowLayoutWidget();
			notificationSettingsContainer.HAnchor |= HAnchor.ParentLeftRight;
			notificationSettingsContainer.VAnchor |= Agg.UI.VAnchor.ParentCenter;
			notificationSettingsContainer.Margin = new BorderDouble(0, 0, 0, 0);
			notificationSettingsContainer.Padding = new BorderDouble(0);

			this.textImageButtonFactory.FixedHeight = TallButtonHeight;

			Agg.Image.ImageBuffer notificationSettingsImage = StaticData.Instance.LoadIcon(Path.Combine("PrintStatusControls", "notify-24x24.png"));
			if (!ActiveTheme.Instance.IsDarkTheme)
			{
				InvertLightness.DoInvertLightness(notificationSettingsImage);
			}

			ImageWidget notificationSettingsIcon = new ImageWidget(notificationSettingsImage);
			notificationSettingsIcon.Margin = new BorderDouble(right: 6, bottom: 6);

			configureNotificationSettingsButton = textImageButtonFactory.Generate("Configure".Localize().ToUpper());
			configureNotificationSettingsButton.Name = "Configure Notification Settings Button";
			configureNotificationSettingsButton.Margin = new BorderDouble(left: 6);
			configureNotificationSettingsButton.VAnchor = VAnchor.ParentCenter;
			configureNotificationSettingsButton.Click += new EventHandler(configureNotificationSettingsButton_Click);

			notificationSettingsLabel = new TextWidget(LocalizedString.Get("Notification Settings"));
			notificationSettingsLabel.AutoExpandBoundsToText = true;
			notificationSettingsLabel.TextColor = ActiveTheme.Instance.PrimaryTextColor;
			notificationSettingsLabel.VAnchor = VAnchor.ParentCenter;

			GuiWidget printNotificationsSwitchContainer = new FlowLayoutWidget();
			printNotificationsSwitchContainer.VAnchor = VAnchor.ParentCenter;
			printNotificationsSwitchContainer.Margin = new BorderDouble(left: 16);

			CheckBox enablePrintNotificationsSwitch = ImageButtonFactory.CreateToggleSwitch(UserSettings.Instance.get("PrintNotificationsEnabled") == "true");
			enablePrintNotificationsSwitch.VAnchor = VAnchor.ParentCenter;
			enablePrintNotificationsSwitch.CheckedStateChanged += (sender, e) =>
			{
				UserSettings.Instance.set("PrintNotificationsEnabled", enablePrintNotificationsSwitch.Checked ? "true" : "false");
			};
			printNotificationsSwitchContainer.AddChild(enablePrintNotificationsSwitch);
			printNotificationsSwitchContainer.SetBoundsToEncloseChildren();

			notificationSettingsContainer.AddChild(notificationSettingsIcon);
			notificationSettingsContainer.AddChild(notificationSettingsLabel);
			notificationSettingsContainer.AddChild(new HorizontalSpacer());
			notificationSettingsContainer.AddChild(configureNotificationSettingsButton);
			notificationSettingsContainer.AddChild(printNotificationsSwitchContainer);

			return notificationSettingsContainer;
		}

		public override void OnClosed(EventArgs e)
		{
			if (unregisterEvents != null)
			{
				unregisterEvents(this, null);
			}
			base.OnClosed(e);
		}

        public static Action openUserDashBoardFunction = null;

        private void cloudSyncGoButton_Click(object sender, EventArgs mouseEvent)
        {
            if(openUserDashBoardFunction != null)
            {
                UiThread.RunOnIdle(openUserDashBoardFunction);
            }
        }

		private event EventHandler unregisterEvents;

		private void AddHandlers()
		{
			PrinterConnectionAndCommunication.Instance.CommunicationStateChanged.RegisterEvent(onPrinterStatusChanged, ref unregisterEvents);
			PrinterConnectionAndCommunication.Instance.EnableChanged.RegisterEvent(onPrinterStatusChanged, ref unregisterEvents);
		}

		private void onPrinterStatusChanged(object sender, EventArgs e)
		{
			this.Invalidate();
		}

		public static Action openPrintNotificationFunction = null;

		private void configureNotificationSettingsButton_Click(object sender, EventArgs mouseEvent)
		{
			if (openPrintNotificationFunction != null)
			{
				UiThread.RunOnIdle(openPrintNotificationFunction);
			}
		}

	}
}