using System;
using System.Collections.Generic;
using System.Text;

namespace Com.Synopsys.Integration.Nuget.Model
{
    public class PackageId
    {
        public string Name { get; set; }
        public string Version { get; set; }

        public PackageId(string name, string version)
        {
            Name = name;
            Version = version;
        }

        public override int GetHashCode()
        {
            int prime = 37;
            int result = 1;
            result = result * prime + ((Name == null) ? 0 : Name.GetHashCode());
            result = result * prime + ((Version == null) ? 0 : Version.GetHashCode());
            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || this.GetType() != obj.GetType())
            {
                return false;
            }
            else
            {
                PackageId other = (PackageId)obj;
                if (Name == null)
                {
                    if (other.Name != null)
                    {
                        return false;
                    }
                }
                else if (!Name.Equals(other.Name))
                {
                    return false;
                }

                if (Version == null)
                {
                    if (other.Version != null)
                    {
                        return false;
                    }
                }
                else if (!Version.Equals(other.Version))
                {
                    return false;
                }
                return true;
            }
        }


    }
}
