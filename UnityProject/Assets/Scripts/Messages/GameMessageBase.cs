﻿using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using Mirror;

public abstract class GameMessageBase : MessageBase
{
	public GameObject NetworkObject;
	public GameObject[] NetworkObjects;

	public abstract IEnumerator Process();

	public virtual IEnumerator Process( NetworkConnection sentBy )
	{
		yield return Process();
	}

	protected IEnumerator WaitFor(uint id)
	{
		if (id == NetId.Empty)
		{
			Logger.LogWarningFormat( "{0} tried to wait on an empty (0) id", Category.NetMessage, this.GetType().Name );
			yield break;
		}

		int tries = 0;
		while ((NetworkObject = ClientScene.FindLocalObject(id)) == null)
		{
			if (tries++ > 10)
			{
				Logger.LogWarningFormat( "{0} could not find object with id {1}", Category.NetMessage, this.GetType().Name, id );
				yield break;
			}

			yield return global::WaitFor.EndOfFrame;
		}
	}

	protected short GetMessageType()
	{
		const BindingFlags FLAGS = BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public;
		FieldInfo field = this.GetType().GetField("MessageType", FLAGS);
		return (short) field.GetValue(null);
	}

	protected IEnumerator WaitFor(params uint[] ids)
	{
		NetworkObjects = new GameObject[ids.Length];

		while (!AllLoaded(ids))
		{
			yield return global::WaitFor.EndOfFrame;
		}
	}

	private bool AllLoaded(uint[] ids)
	{
		for (int i = 0; i < ids.Length; i++)
		{
			var netId = ids[i];
			if ( netId == NetId.Invalid ) {
				continue;
			}
			GameObject obj = ClientScene.FindLocalObject(netId);
			if (obj == null)
			{
				return false;
			}

			NetworkObjects[i] = obj;
		}

		return true;
	}
}
