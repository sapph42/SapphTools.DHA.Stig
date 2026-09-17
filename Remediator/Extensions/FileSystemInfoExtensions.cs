using SapphTools.SecurityDescriptor;
using System.Diagnostics;
using System.Security.AccessControl;

namespace SapphTools.DHA.Stig.Remediator.Extensions; 
public static class FileSystemInfoExtensions {
    public static string GetSecurityDescriptorSddlForm(this FileSystemInfo info) {
        if (info is DirectoryInfo di) {
            return di.GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.All);
        } else if (info is FileInfo fi) {
            return fi.GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.All);
        } else {
            throw new UnreachableException();
        }
    }
    public static Sddl GetSecurityDescriptorSddl(this FileSystemInfo info) {
        if (info is DirectoryInfo di) {
            return new (
                di.GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.All),
                SecurityDescriptor.Enums.ObjectType.File
            );
        } else if (info is FileInfo fi) {
            return new(
                fi.GetAccessControl().GetSecurityDescriptorSddlForm(AccessControlSections.All),
                SecurityDescriptor.Enums.ObjectType.File
            );
        } else {
            throw new UnreachableException();
        }
    }
    public static void SetSecurityDescriptorSddlForm(this FileSystemInfo info, string sddlForm) {
        ObjectSecurity sec;
        if (info is DirectoryInfo di) {
            sec = di.GetAccessControl();
        } else if (info is FileInfo fi) {
            sec = fi.GetAccessControl();
        } else {
            throw new UnreachableException();
        }
        sec.SetSecurityDescriptorSddlForm(sddlForm);
    }
    public static void SetSecurityDescriptorSddlForm(this FileSystemInfo info, Sddl sddl) {
        ObjectSecurity sec;
        if (info is DirectoryInfo di) {
            sec = di.GetAccessControl();
        } else if (info is FileInfo fi) {
            sec = fi.GetAccessControl();
        } else {
            throw new UnreachableException();
        }
        sec.SetSecurityDescriptorSddlForm(sddl.ToString());
    }
}
