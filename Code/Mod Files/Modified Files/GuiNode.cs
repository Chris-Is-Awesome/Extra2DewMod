using System;
using System.Collections.Generic;
using UnityEngine;

public class GuiNode : MonoBehaviour
{
	//Made public
	public string _trackId;

	Dictionary<Type, IGuiEvent> storedEvents = new Dictionary<Type, IGuiEvent>();

	List<GuiContentApplier> savedAppliers;

	List<GuiNode> savedChildren;

	public object ObjectTag { get; set; }

	static List<GuiContentApplier> GatherAppliers(Transform root, List<GuiContentApplier> list)
	{
		if (list == null)
		{
			list = new List<GuiContentApplier>();
		}
		list.AddRange(root.GetComponents<GuiContentApplier>());
		for (int i = root.childCount - 1; i >= 0; i--)
		{
			Transform child = root.GetChild(i);
			if (child.GetComponent<GuiNode>() == null)
			{
				GuiNode.GatherAppliers(child, list);
			}
		}
		return list;
	}

	static List<GuiNode> GatherChildren(Transform root, List<GuiNode> list)
	{
		if (list == null)
		{
			list = new List<GuiNode>();
		}
		for (int i = root.childCount - 1; i >= 0; i--)
		{
			Transform child = root.GetChild(i);
			GuiNode component = child.GetComponent<GuiNode>();
			if (component == null)
			{
				GuiNode.GatherChildren(child, list);
			}
			else
			{
				list.Add(component);
			}
		}
		return list;
	}

	List<GuiContentApplier> GetAppliers()
	{
		if (this.savedAppliers == null)
		{
			this.savedAppliers = GuiNode.GatherAppliers(base.transform, null);
			for (int i = this.savedAppliers.Count - 1; i >= 0; i--)
			{
				this.savedAppliers[i].Enable(this);
			}
		}
		return this.savedAppliers;
	}

	List<GuiNode> GetChildren()
	{
		if (this.savedChildren == null)
		{
			this.savedChildren = GuiNode.GatherChildren(base.transform, null);
		}
		return this.savedChildren;
	}

	protected virtual void OnConnected(GuiBindInData inData, GuiBindData outData)
	{
	}

	void Connect(GuiBindInData inData, GuiBindData outData)
	{
		if (!string.IsNullOrEmpty(this._trackId))
		{
			outData.AddTracker(this._trackId, this);
		}
		this.ApplyContent(inData.Content, false);
		this.OnConnected(inData, outData);
	}

	public void ApplyContent(GuiContentData content, bool withChildren = false)
	{
		List<GuiContentApplier> appliers = this.GetAppliers();
		for (int i = appliers.Count - 1; i >= 0; i--)
		{
			appliers[i].Apply(content);
		}
		if (withChildren)
		{
			List<GuiNode> children = this.GetChildren();
			for (int j = children.Count - 1; j >= 0; j--)
			{
				children[j].ApplyContent(content, true);
			}
		}
	}

	public GuiNode GetNodeById(string id)
	{
		if (this._trackId == id)
		{
			return this;
		}
		GuiNode[] componentsInChildren = base.GetComponentsInChildren<GuiNode>();
		for (int i = componentsInChildren.Length - 1; i >= 0; i--)
		{
			if (componentsInChildren[i]._trackId == id)
			{
				return componentsInChildren[i];
			}
		}
		return null;
	}

	public GuiNode GetNodeByTag(object tag)
	{
		if (tag == this.ObjectTag)
		{
			return this;
		}
		if (tag == null)
		{
			return null;
		}
		GuiNode[] componentsInChildren = base.GetComponentsInChildren<GuiNode>();
		for (int i = componentsInChildren.Length - 1; i >= 0; i--)
		{
			if (tag.Equals(componentsInChildren[i].ObjectTag))
			{
				return componentsInChildren[i];
			}
		}
		return null;
	}

	public void AppendChild(GuiNode node)
	{
		TransformUtility.SetParent(node.transform, base.transform, true);
	}

	public void ResetChildren()
	{
		this.savedChildren = null;
		this.savedAppliers = null;
	}

	public T GetEvent<T>() where T : class, IGuiEvent
	{
		IGuiEvent guiEvent;
		if (this.storedEvents.TryGetValue(typeof(T), out guiEvent) && guiEvent != null)
		{
			return guiEvent as T;
		}
		T componentInterface = GameObjectUtility.GetComponentInterface<T>(base.gameObject);
		if (componentInterface != null)
		{
			this.storedEvents[typeof(T)] = componentInterface;
		}
		return componentInterface;
	}

	public static GuiBindData Connect(GameObject obj, GuiBindInData inData)
	{
		GuiBindData guiBindData = new GuiBindData();
		GuiNode[] componentsInChildren = obj.GetComponentsInChildren<GuiNode>(true);
		for (int i = componentsInChildren.Length - 1; i >= 0; i--)
		{
			componentsInChildren[i].Connect(inData, guiBindData);
		}
		return guiBindData;
	}

	public static GuiBindData CreateAndConnect(GameObject prefab, GuiBindInData inData)
	{
		GameObject obj = GameObjectUtility.TransformInstantiate(prefab, inData.Root);
		return GuiNode.Connect(obj, inData);
	}

	public static void ApplyData(GameObject obj, GuiContentData content)
	{
		GuiNode[] componentsInChildren = obj.GetComponentsInChildren<GuiNode>(true);
		for (int i = componentsInChildren.Length - 1; i >= 0; i--)
		{
			componentsInChildren[i].ApplyContent(content, false);
		}
	}

	public delegate void OnVoidFunc(object context);

	public delegate void OnFloatFunc(float value, object context);

	public delegate void OnBoolFunc(bool value, object context);

	public class VoidBinding
	{
		public GuiNode.OnVoidFunc func;

		public object context;

		public VoidBinding(GuiNode.OnVoidFunc f, object c)
		{
			this.func = f;
			this.context = c;
		}
	}

	public class FloatBinding
	{
		public GuiNode.OnFloatFunc func;

		public object context;

		public FloatBinding(GuiNode.OnFloatFunc f, object c)
		{
			this.func = f;
			this.context = c;
		}
	}

	public class BoolBinding
	{
		public GuiNode.OnBoolFunc func;

		public object context;

		public BoolBinding(GuiNode.OnBoolFunc f, object c)
		{
			this.func = f;
			this.context = c;
		}
	}
}
