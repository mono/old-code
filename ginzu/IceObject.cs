// -*- mode: csharp; c-basic-offset: 2; indent-tabs-mode: nil -*-
//
// IceObject.cs
//
// Written by:
//   Vladimir Vukicevic <vladimir@sparklestudios.com>
//
// Copyright (C) 2003 Sparkle Studios, LLC
//
// This file is distributed under the terms of the license
// agreement contained in the LICENSE file in the top level
// of this distribution.
//

using System;
using System.Runtime.InteropServices;
using System.Collections;

namespace Ice {

  public class FacetMap : Dictionary {
    public FacetMap () : base (typeof(string), typeof(Ice.Object)) { }
  }

  // Ice.Object
  //
  // The root of the Ice inheritance hierarchy.  Note that StructLayout
  // CANNOT be applied to this, since its parent class does not have the
  // attribute; doing so causes a TypeLoadException under the MS CLR,
  // but runs fine under the Mono runtime.
  //
  // In the future, Ice.Object functionality may be virtualized, such that
  // classes can inherit directly from MarshalByRefObject or
  // ContextBoundObject, as desired.

  //[StructLayout(LayoutKind.Sequential)]
  public class Object : System.MarshalByRefObject {
    // facetmap is really a:
    public FacetMap _activeFacetMap;

    public Object()
    {
      _activeFacetMap = new FacetMap();
    }

    [Ice.OperationModeAttribute(OperationMode.Nonmutating)]
    public void ice_ping () {
      // do nothing
    }

    [Ice.OperationModeAttribute(OperationMode.Nonmutating)]
    public bool ice_isA (string name) {
      Type t = this.GetType();
      Type o = IceUtil.IceNameToType (name);
      return (t == o || t.IsSubclassOf(o));
    }

    [Ice.OperationModeAttribute(OperationMode.Nonmutating)]
    public string[] ice_ids () {

      ArrayList typenames = new ArrayList();
      Type t = this.GetType();
      while (t != typeof(Ice.Object)) {
        typenames.Add (IceUtil.TypeToIceName (t));
        t = t.BaseType;
      }
      typenames.Add ("::Ice::Object"); // blah.


      return (string[]) typenames.ToArray(typeof(string));
    }

    [Ice.OperationModeAttribute(OperationMode.Nonmutating)]
    public string ice_id () {
      return IceUtil.TypeToIceName (this.GetType());
    }

    [Ice.OperationModeAttribute(OperationMode.Nonmutating)]
    public string[] ice_facets () {
      string[] facets = null;
      lock (_activeFacetMap) {
        facets = new string[_activeFacetMap.Count];
        int i = 0;
        IDictionaryEnumerator iter = _activeFacetMap.GetEnumerator();
        while (iter.MoveNext()) {
          facets[i++] = (string) iter.Key;
        }
      }

      return facets;
    }

    /*
    private void ice_cacheTypeNames() {
      ice_cachedTypeNames = new ArrayList();
      Type t = this.GetType();
      while (t != typeof(Ice.Object)) {
        ice_cachedTypeNames.Add (IceUtil.TypeToIceName (t));
        t = t.BaseType;
      }
      // finally add Ice.Object
      // ice_cachedTypeNames.Add (IceUtil.TypeToIceName (t));
      ice_cachedTypeNames.Add ("::Ice::Object"); // blah.
    }
    */

  }

}
