using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using TinyNvidiaUpdateChecker.Handlers;

namespace TinyNvidiaUpdateChecker.Forms
{
    public partial class ComponentChooserForm : Form
    {
        List<Component> componentList;
        List<string> chosenComponents = [];
        string[] configComponents = [];
        int driverIdx = 0;

        public ComponentChooserForm()
        {
            InitializeComponent();
        }

        public (List<string>, bool saveConfig) OpenForm(List<Component> componentList, string configComponentsString = null)
        {
            this.componentList = componentList;

            // Parse configComponentsString into an array if it exists
            if (configComponentsString != null)
            {
                configComponents = configComponentsString.Split(", ", StringSplitOptions.RemoveEmptyEntries);
            }

            // If quiet mode + config entry exist, return latest used co
            if (!MainConsole.showUI && configComponents != null)
            {
                return (configComponents.ToList(), false);
            }

            // If config entry does not exist, hide the latest used components link
            if (configComponentsString == null) latestLabel.Visible = false;

            ShowDialog();

            return (chosenComponents, true);
        }

        private void ComponentChooserForm_Load(object sender, EventArgs e)
        {
            foreach (Component component in componentList)
            {
                string label = component.label;
                int idx = checkedListBox.Items.Add(label);
                component.index = idx;

                if (component.name == "Display.Driver") { driverIdx = idx; }
            }

            // Apply last used as default
            if (configComponents.Length > 0) {
                latestLabel_LinkClicked(latestLabel, new LinkLabelLinkClickedEventArgs(latestLabel.Links[0]));
            }
        }

        private void checkedListBox_SelectedValueChanged(object sender, EventArgs e)
        {
            Component comp = componentList.First(x => x.index == checkedListBox.SelectedIndex);
            string description = ComponentHandler.GetComponentDescription(comp.name);

            if (comp.dependencies.Count > 0)
            {
                bool first = true;

                foreach (KeyValuePair<string, string> dependency in comp.dependencies)
                {
                    if (first)
                    {
                        first = false;
                        description += "\n\nRequires:\n";
                    }

                    description += $"{dependency.Value}\n";
                }
            }

            richTextBox.Text = description;
            richTextBox.Text += $"\n\nComponent version: {comp.version}";
        }

        private void okButton_Click(object sender, EventArgs e)
        {
            Enabled = false;
            chosenComponents.Clear();

            Dictionary<string, bool> dependencyList = new() {
                {"Display.Driver", checkedListBox.CheckedIndices.Contains(driverIdx)}
            };

            foreach (int idx in checkedListBox.CheckedIndices)
            {
                Component comp = componentList.First(x => x.index >= idx);
                chosenComponents.Add(comp.name);

                foreach (KeyValuePair<string, string> dependency in comp.dependencies)
                {
                    dependencyList.TryAdd(dependency.Key, false);
                }
            }

            foreach (int idx in checkedListBox.CheckedIndices)
            {
                Component comp = componentList.First(x => x.index >= idx);

                if (dependencyList.ContainsKey(comp.name))
                {
                    dependencyList[comp.name] = true;
                }
            }

            bool canProceed = dependencyList.Count(x => x.Value == true) == dependencyList.Count;

            if (canProceed)
            {
                Close();
            }
            else
            {
                string missingComponents = string.Join("\n", dependencyList.Where(x => !x.Value).Select(x => ComponentHandler.GetComponentLabelFromName(x.Key)));
                string message = $"You are missing components, please review your selection.\n\nMissing required components:\n{missingComponents}";
                MessageBox.Show(message, "TinyNvidiaUpdateChecker", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            Enabled = true;
        }

        private void noneLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                checkedListBox.SetItemChecked(i, false);
            }

            richTextBox.Text = "";
        }

        private void allLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            for (int i = 0; i < checkedListBox.Items.Count; i++)
            {
                checkedListBox.SetItemChecked(i, true);
            }
        }

        // Loop through configComponents and apply to components list box
        private void latestLabel_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            foreach (Component component in componentList)
            {
                int index = component.index;
                bool isInConfig = configComponents.Contains(component.name);
                checkedListBox.SetItemChecked(index, isInConfig);
            }
        }
    }
}
