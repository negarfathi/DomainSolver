(set-info :smt-lib-version 2.6)
;;; Processed by pysmt to remove constant-real bitvector literals
(set-logic QF_FP)
(set-info :source |SPARK inspired floating point problems by Florian Schanda and Martin Brain|)
(set-info :category "crafted")
(set-info :status unsat)
(define-fun is_finite ((f Float32)) Bool (or (fp.isZero f) (fp.isNormal f) (fp.isSubnormal f)))
(declare-fun a () Float32)
(declare-fun b () Float32)
(assert (is_finite a))
(assert (is_finite b))
(assert (not (or (fp.leq a b) (fp.leq b a))))
(check-sat)
(exit)
