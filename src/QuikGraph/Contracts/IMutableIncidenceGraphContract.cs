﻿using System;
using System.Collections.Generic;
#if SUPPORTS_CONTRACTS
using System.Diagnostics.Contracts;
#endif
using System.Linq;

namespace QuikGraph.Contracts
{
#if SUPPORTS_CONTRACTS
    [ContractClassFor(typeof(IMutableIncidenceGraph<,>))]
#endif
    abstract class IMutableIncidenceGraphContract<TVertex, TEdge>
        : IMutableIncidenceGraph<TVertex, TEdge>
        where TEdge : IEdge<TVertex>
    {
        #region IMutableIncidenceGraph<TVertex,TEdge> Members

        int IMutableIncidenceGraph<TVertex, TEdge>.RemoveOutEdgeIf(
            TVertex v,
            EdgePredicate<TVertex, TEdge> predicate)
        {
            IMutableIncidenceGraph<TVertex, TEdge> ithis = this;
#if SUPPORTS_CONTRACTS
            Contract.Requires(v != null);
            Contract.Requires(ithis.ContainsVertex(v));
            Contract.Requires(predicate != null);
            Contract.Ensures(Contract.Result<int>() == Contract.OldValue(Enumerable.Count(ithis.OutEdges(v), ve => predicate(ve))));
            Contract.Ensures(Enumerable.All(ithis.OutEdges(v), ve => !predicate(ve)));
#endif

            return default(int);
        }

        void IMutableIncidenceGraph<TVertex, TEdge>.ClearOutEdges(TVertex v)
        {
            IMutableIncidenceGraph<TVertex, TEdge> ithis = this;
#if SUPPORTS_CONTRACTS
            Contract.Requires(v != null);
            Contract.Requires(ithis.ContainsVertex(v));
            Contract.Ensures(ithis.OutDegree(v) == 0);
#endif
        }

        void IMutableIncidenceGraph<TVertex, TEdge>.TrimEdgeExcess()
        { }
#endregion

#region IMutableGraph<TVertex,TEdge> Members

        void IMutableGraph<TVertex, TEdge>.Clear()
        {
            throw new NotImplementedException();
        }

#endregion

#region IGraph<TVertex,TEdge> Members

        bool IGraph<TVertex, TEdge>.IsDirected
        {
            get { throw new NotImplementedException(); }
        }

        bool IGraph<TVertex, TEdge>.AllowParallelEdges
        {
            get { throw new NotImplementedException(); }
        }

#endregion

#region IIncidenceGraph<TVertex,TEdge> Members

        bool IIncidenceGraph<TVertex, TEdge>.ContainsEdge(TVertex source, TVertex target)
        {
            throw new NotImplementedException();
        }

        bool IIncidenceGraph<TVertex, TEdge>.TryGetEdges(TVertex source, TVertex target, out IEnumerable<TEdge> edges)
        {
            throw new NotImplementedException();
        }

        bool IIncidenceGraph<TVertex, TEdge>.TryGetEdge(TVertex source, TVertex target, out TEdge edge)
        {
            throw new NotImplementedException();
        }

#endregion

#region IImplicitGraph<TVertex,TEdge> Members

        bool IImplicitGraph<TVertex, TEdge>.IsOutEdgesEmpty(TVertex v)
        {
            throw new NotImplementedException();
        }

        int IImplicitGraph<TVertex, TEdge>.OutDegree(TVertex v)
        {
            throw new NotImplementedException();
        }

        IEnumerable<TEdge> IImplicitGraph<TVertex, TEdge>.OutEdges(TVertex v)
        {
            throw new NotImplementedException();
        }

        bool IImplicitGraph<TVertex, TEdge>.TryGetOutEdges(TVertex v, out IEnumerable<TEdge> edges)
        {
            throw new NotImplementedException();
        }

        TEdge IImplicitGraph<TVertex, TEdge>.OutEdge(TVertex v, int index)
        {
            throw new NotImplementedException();
        }

#endregion

#region IImplicitVertexSet<TVertex> Members
        bool IImplicitVertexSet<TVertex>.ContainsVertex(TVertex vertex)
        {
            throw new NotImplementedException();
        }

#endregion
    }
}