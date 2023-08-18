using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KSP.UI.Screens;

namespace SPEngine
{
	[KSPAddon(KSPAddon.Startup.AllGameScenes, false)]
	public class Core : MonoBehaviour
	{
		public static Core Instance { get; protected set; }
		private ApplicationLauncherButton button;
		private UI.MasterWindow masterWindow;
		private UI.ConfigWindow configWindow;
		public DesignLibrary library;
		public Dictionary<char, Family> families;

		public void Start()
		{
			if (Instance != null) {
				Destroy(this);
				return;
			}

			Instance = this;
			library = new DesignLibrary();
			masterWindow = new UI.MasterWindow();
			families = new Dictionary<char, Family>();
			foreach (ConfigNode cn in GameDatabase.Instance.GetConfigNodes("SPEFamily")) {
				try {
					Family f = new Family(cn);
					families.Add(f.letter, f);
				} catch (Exception ex) {
					Logging.LogException(ex);
				}
			}
			if (ScenarioSPEngine.Instance != null)
				Load(ScenarioSPEngine.Instance.node);
			Logging.Log("Core loaded successfully");
		}

		public void EditPart(ModuleSPEngine m)
		{
			if (configWindow != null) {
				configWindow.Hide();
				configWindow = null;
				return;
			}
			configWindow = new UI.ConfigWindow(m);
			configWindow.Show();
		}

		protected void Awake()
		{
			try {
				GameEvents.onGUIApplicationLauncherReady.Add(this.OnGuiAppLauncherReady);
			} catch (Exception ex) {
				Logging.LogException(ex);
			}
		}

		public void OnGUI()
		{
			GUI.depth = 0;

			Action windows = delegate { };
			foreach (var window in UI.AbstractWindow.Windows.Values)
				windows += window.Draw;
			windows.Invoke();
		}

		private void OnGuiAppLauncherReady()
		{
			try {
				button = ApplicationLauncher.Instance.AddModApplication(
					masterWindow.Show,
					HideGUI,
					null,
					null,
					null,
					null,
					ApplicationLauncher.AppScenes.VAB | ApplicationLauncher.AppScenes.SPH,
					GameDatabase.Instance.GetTexture("SPEngine/Textures/toolbar_icon", false));
				GameEvents.onGameSceneLoadRequested.Add(this.OnSceneChange);
			} catch (Exception ex) {
				Logging.LogException(ex);
				Logging.LogFormat("{0}mW", masterWindow == null ? "no " : "");
			}
		}

		private void HideGUI()
		{
			masterWindow.Hide();
			if (configWindow != null) {
				configWindow.Hide();
				configWindow = null;
			}
		}

		private void OnSceneChange(GameScenes s)
		{
			HideGUI();
		}

		public void OnDestroy()
		{
			Instance = null;
			try {
				GameEvents.onGUIApplicationLauncherReady.Remove(this.OnGuiAppLauncherReady);
				if (button != null) {
					GameEvents.onGameSceneLoadRequested.Remove(this.OnSceneChange);
					ApplicationLauncher.Instance.RemoveModApplication(button);
				}
			} catch (Exception ex) {
				Logging.LogException(ex);
			}
		}

		private void SaveTechLevels(ConfigNode node)
		{
			foreach (Family f in families.Values) {
				node.AddValue(f.letter.ToString(), f.unlocked.ToString());
			}
		}

		private void LoadTechLevels(ConfigNode node)
		{
			foreach (Family f in families.Values) {
				f.unlocked = 0;
				if (node != null && node.HasValue(f.letter.ToString()))
					int.TryParse(node.GetValue(f.letter.ToString()), out f.unlocked);
			}
		}

		public void Save(ConfigNode node)
		{
			Logging.Log("Saving library");
			ConfigNode ln = node.AddNode("library");
			library.Save(ln);
			ConfigNode tn = node.AddNode("techLevels");
			SaveTechLevels(tn);
		}

		public void Load(ConfigNode node)
		{
			if (node.HasNode("library"))
				library.Load(node.GetNode("library"));
			LoadTechLevels(node.GetNode("techLevels"));
		}
	}

	[KSPScenario(ScenarioCreationOptions.AddToAllGames, GameScenes.FLIGHT, GameScenes.EDITOR, GameScenes.SPACECENTER, GameScenes.TRACKSTATION)]
	public class ScenarioSPEngine : ScenarioModule
	{
		public static ScenarioSPEngine Instance {get; protected set; }
		public ConfigNode node;

		public override void OnAwake()
		{
			Instance = this;
			base.OnAwake();
		}

		public override void OnSave(ConfigNode node)
		{
			Core.Instance.Save(node);
		}

		public override void OnLoad(ConfigNode node)
		{
			this.node = node;
			if (Core.Instance != null)
				Core.Instance.Load(node);
		}
	}
}
