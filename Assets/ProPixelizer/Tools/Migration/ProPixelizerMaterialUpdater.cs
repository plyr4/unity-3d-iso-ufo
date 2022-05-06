// Copyright Elliot Bentine, 2022-
#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;

namespace ProPixelizer.Tools.Migration
{
    /// <summary>
    /// Checks if a material has properties that require migration between versions of ProPixelizer.
    /// </summary>
    public abstract class ProPixelizerMaterialUpdater
    {
        public virtual bool CheckForUpdate(SerializedObject so)
        {
            var renamedProps = GetMigratedProperties();
            bool needsUpdate = false;
            foreach (var prop in renamedProps)
                if (prop.CheckForUpdate(so))
                    return true;
            return needsUpdate;
        }

        public virtual void DoUpdate(SerializedObject so)
        {
            var renamedProps = GetMigratedProperties();
            foreach (var prop in renamedProps)
                prop.DoUpdate(so);
        }

        public abstract List<IMigratedProperty> GetMigratedProperties();
    }

    public interface IMigratedProperty {
        bool CheckForUpdate(SerializedObject so);
        void DoUpdate(SerializedObject so);
    }

    public abstract class RenamedProperty : IMigratedProperty
    {
        public string OldName;
        public string NewName;
        public abstract string GetArrayName();

        public RenamedProperty() { }

        /// <summary>
        /// Get the index of the old property name in the property array.
        /// </summary>
        public int GetPropertyIndex(SerializedObject so, string property)
        {
            var propertyList = so.FindProperty("m_SavedProperties");
            if (propertyList == null)
                return -1;
            var properties = propertyList.FindPropertyRelative(GetArrayName());
            if (properties != null)
            {
                for (int i = 0; i < properties.arraySize; i++)
                {
                    var tex = properties.GetArrayElementAtIndex(i);
                    if (tex.displayName == property)
                    {
                        return i;
                    }
                }
            }
            return -1;
        }

        public bool CheckForUpdate(SerializedObject so)
        {
            return GetPropertyIndex(so, OldName) != -1;
        }

        public void DoUpdate(SerializedObject so)
        {
            if (!CheckForUpdate(so))
                return;
            var props = so.FindProperty("m_SavedProperties").FindPropertyRelative(GetArrayName());

            // Remove the new property if it exists
            int newIndex = GetPropertyIndex(so, NewName);
            if (newIndex != -1)
                props.DeleteArrayElementAtIndex(newIndex);

            int oldIndex = GetPropertyIndex(so, OldName);
            if (oldIndex == -1)
                return;

            Rename(props.GetArrayElementAtIndex(oldIndex));
            so.ApplyModifiedPropertiesWithoutUndo();
        }

        public void Rename(SerializedProperty prop)
        {
            prop.FindPropertyRelative("first").stringValue = NewName;
        }
    }

    public class RenamedTexture : RenamedProperty
    {
        public override string GetArrayName() => "m_TexEnvs";
    }

    public class RenamedColor : RenamedProperty
    {
        public override string GetArrayName() => "m_Colors";
    }

    public class RenamedFloat : RenamedProperty
    {
        public override string GetArrayName() => "m_Floats";
    }
}
#endif