using System;
using System.Collections.Generic;

public interface IQueryAttributable
{
    void ApplyQueryAttributes(IDictionary<string, object> query);
}

