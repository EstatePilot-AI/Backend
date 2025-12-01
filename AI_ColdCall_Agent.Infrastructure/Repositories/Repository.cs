using Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace Repositories;

public class Repository<T>: IRepository<T> where T : class
{
}
