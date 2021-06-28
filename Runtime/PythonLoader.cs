/*
This program is free software; you can redistribute it and/or
modify it under the terms of the GNU General Public License
as published by the Free Software Foundation; either version 2
of the License, or (at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with this program; if not, write to the Free Software Foundation,
Inc., 51 Franklin Street, Fifth Floor, Boston, MA 02110-1301, USA.

The Original Code is Copyright (C) 2020 Voxell Technologies and Contributors.
All rights reserved.
*/

using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Threading;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Voxell.PythonVX
{
  [System.Serializable]
  public class PythonLoader
  {
    public ScriptEngine pythonEngine;
    public ScriptScope scriptScope;
    public string libPath;
    public string execPath;
    public string[] libDirectories;
    public string[] execDirectories;

    public bool executionComplete = false;

    /// <summary>
    /// Execute python script in a new thread to prevent from stalling the main thread
    /// </summary>
    /// <param name="pythonAsset"></param>
    /// <returns></returns>
    public Thread ThreadedExecuteScript(PythonAsset pythonAsset)
    {
      executionComplete = false;
      Debug.Log($"Executing python script: {pythonAsset}");
      // scriptScope = pythonEngine.ExecuteFile(pythonAsset.filePath);
      Thread pythonThread = new Thread(new ParameterizedThreadStart(ExecuteScriptTask));
      pythonThread.Start(pythonAsset);
      return pythonThread;
    }

    private void ExecuteScriptTask(object _asset)
    {
      PythonAsset asset = _asset as PythonAsset;
      scriptScope = pythonEngine.ExecuteFile(asset.filePath);
      executionComplete = true;
    }

    /// <summary>
    /// Initialize python library directories (this function is only meant to be called once)
    /// </summary>
    public void Init()
    {
      // path to the python standard library
      libPath = FileUtil.projectPath + "Packages/UnityIronPython/Runtime/IronPython/Lib/";
      libDirectories = Directory.GetDirectories(libPath);
    }

    /// <summary>
    /// Setup python interpreter and execution libraries
    /// </summary>
    /// <param name="types">datatypes to add</param>
    /// <param name="rootFolderPath">path to the root folder where your main python script lives</param>
    public void Setup(string execPath, ISet<Type> types=null)
    {
      pythonEngine = Python.CreateEngine();
      if (types != null)
      {
        if ( !types.Contains(typeof(GameObject)) ) pythonEngine.Runtime.LoadAssembly(typeof(GameObject).Assembly);
        foreach (Type type in types) pythonEngine.Runtime.LoadAssembly(type.Assembly);
      } else pythonEngine.Runtime.LoadAssembly(typeof(GameObject).Assembly);
      // add standard library paths
      ICollection<string> searchPaths = pythonEngine.GetSearchPaths();
      foreach (string path in libDirectories) searchPaths.Add(path);

      this.execPath = execPath;
      execDirectories = Directory.GetDirectories(this.execPath);
      foreach (string path in execDirectories) searchPaths.Add(path);

      pythonEngine.SetSearchPaths(searchPaths);
    }
  }
}
