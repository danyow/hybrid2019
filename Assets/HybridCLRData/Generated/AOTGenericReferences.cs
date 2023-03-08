public class AOTGenericReferences : UnityEngine.MonoBehaviour
{

	// {{ constraint implement type
	// }} 

	// {{ AOT generic type
	//System.Collections.Generic.IEnumerator`1<System.Object>
	//System.Collections.Generic.List`1<HotUpdateEntry/MyIntVec3>
	//System.Collections.Generic.List`1<System.Object>
	//System.Collections.Generic.List`1<HotUpdateMono/MyIntVec3>
	//System.Collections.Generic.List`1/Enumerator<System.Object>
	// }}

	public void RefMethods()
	{
		// HotUpdateEntry/MyIntVec3 System.Linq.Enumerable::First<HotUpdateEntry/MyIntVec3>(System.Collections.Generic.IEnumerable`1<HotUpdateEntry/MyIntVec3>)
		// HotUpdateMono/MyIntVec3 System.Linq.Enumerable::First<HotUpdateMono/MyIntVec3>(System.Collections.Generic.IEnumerable`1<HotUpdateMono/MyIntVec3>)
	}
}