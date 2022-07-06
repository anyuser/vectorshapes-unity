using UnityEngine;

public static class ComponentUtils
{
	public static T GetOrCreateComponent<T>(GameObject gameObject) where T : Component => gameObject.TryGetComponent<T>(out var c) ? c : gameObject.AddComponent<T>();
}