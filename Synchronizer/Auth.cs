using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Synchronizer
{
    public class Auth : ICompleat.Auth
    {
        #region Fields

        public string hh_email = "no-reply.ic-po@etherlive.co.uk", hh_pword = "P3nnyP1p";

        #endregion Fields

        #region Constructors

        public Auth(string tenantId, string companyId, string key) : base(tenantId, companyId, key)
        {
        }

        #endregion Constructors

        #region Properties

        public static Auth ETHL
        {
            get { return new Auth("3c1e7bda-cb15-4c21-a27a-df537b25c85a", "00b25d7d-431b-4a7b-92ae-f0134e30f976", "edb17d63-85cf-44c5-aae4-3091cd03d7ea"); }
        }

        public static Auth PMY
        {
            get { return new Auth("3c1e7bda-cb15-4c21-a27a-df537b25c85a", "26e05c22-6a2b-4561-b494-ac1e0bab38bc", "edb17d63-85cf-44c5-aae4-3091cd03d7ea"); }
        }

        #endregion Properties
    }
}