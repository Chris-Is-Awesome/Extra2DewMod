using UnityEngine;

namespace ModStuff.CreativeMenu
{
    public class CMW_Filter : CMenuWindow
    {
        public delegate void OnUpdateFilterButton(string name);
        public event OnUpdateFilterButton onUpdateFilterButton;

        UITextInput textInput;
        UICheckBox caseSensitive;
        UICheckBox matchName;
        public override void BuildMenu()
        {
            //Title
            UITextFrame title = UIFactory.Instance.CreateTextFrame(0f, 4f, menuGo.transform, "Objects filter");

            //Content
            textInput = UIFactory.Instance.CreateTextInput(-1.25f, 2.5f, menuGo.transform);

            //Match whole name checkbox
            matchName = UIFactory.Instance.CreateCheckBox(2f, 1.5f, menuGo.transform, "Match name");

            //Case sensitive checkbox
            caseSensitive = UIFactory.Instance.CreateCheckBox(-2f, 1.5f, menuGo.transform, "Case sensitive");
            caseSensitive.onInteraction += delegate (bool box)
            {
                UpdateFilter(textInput.StringValue, caseSensitive.Value, matchName.Value);
            };

            matchName.onInteraction += delegate (bool box)
            {
                UpdateFilter(textInput.StringValue, caseSensitive.Value, matchName.Value);
            };

            //Confirm button
            UIButton accept = UIFactory.Instance.CreateButton(UIButton.ButtonType.Confirm, 2.5f, 2.5f, menuGo.transform, "Confirm");
            accept.ScaleBackground(new Vector2(0.5f, 1f));
            accept.onInteraction += ApplyFilterAndExit;
            textInput.onEnterPressed += ApplyFilterAndExit;
            textInput.onValueChanged += delegate (string text)
            {
                UpdateFilter(textInput.StringValue, caseSensitive.Value, matchName.Value);
            };
        }

        public override void OnOpen()
        {
            UpdateFilter(null, false, false);
            ResetFilterName();
        }

        void ApplyFilterAndExit()
        {
            UpdateFilter(textInput.StringValue, caseSensitive.Value, matchName.Value);
            logic.CloseCurrentMenu();
            textInput.StringValue = null;
        }

        void UpdateFilter(string filter, bool caseSensitive, bool matchName)
        {
            logic.SetFilter(filter, caseSensitive, matchName);
            if (string.IsNullOrEmpty(filter))
            {
                ResetFilterName();
            }
            else
            {
                onUpdateFilterButton?.Invoke("Object filter: " + filter);
            }
            logic.UpdateMarkers(false);
            logic.UpdateMarkers(true);
        }

        public void ResetFilterName()
        {
            onUpdateFilterButton?.Invoke("Object filter: NONE");
        }
    }
}
