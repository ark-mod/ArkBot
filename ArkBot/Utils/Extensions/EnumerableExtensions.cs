﻿using System;
using System.Collections.Generic;

namespace ArkBot.Utils.Extensions
{
    public static class EnumerableExtensions
    {
        //based on code from http://stackoverflow.com/a/6624756
        public static IEnumerable<TResult> Merge<TFirst, TSecond, TResult>(this IEnumerable<TFirst> first,
            IEnumerable<TSecond> second, Func<TFirst, TSecond, TResult> operation)
        {
            using (var iter1 = first.GetEnumerator())
            {
                using (var iter2 = second.GetEnumerator())
                {
                    while (iter1.MoveNext())
                    {
                        if (iter2.MoveNext())
                        {
                            yield return operation(iter1.Current, iter2.Current);
                        }
                        else
                        {
                            yield return operation(iter1.Current, default);
                        }
                    }
                    while (iter2.MoveNext())
                    {
                        yield return operation(default, iter2.Current);
                    }
                }
            }
        }
    }
}
