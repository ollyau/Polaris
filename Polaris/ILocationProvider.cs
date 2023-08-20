using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris
{
	interface ILocationProvider
	{
		public Task<IEnumerable<Location>> GetLocations();
	}
}
