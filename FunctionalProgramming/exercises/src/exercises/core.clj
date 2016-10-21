(ns exercises.core
  (:require [clojure.core.protocols :refer [CollReduce coll-reduce]]
            [clojure.core.reducers :refer [CollFold coll-fold]]))

; day 1
(defn recursive-sum-recur [numbers]
  (loop [acc 0
         remaining-numbers numbers]
    (if (empty? remaining-numbers)
      acc
      (recur (+ acc (first remaining-numbers)) (rest remaining-numbers)))))

(defn reduce-sum [numbers]
  (reduce #(+ %1 %2) 0 numbers))

; day 2
(defn make-reducer [reducible transformf]
  (reify
    CollReduce
    (coll-reduce [_ f1]
      (coll-reduce reducible (transformf f1) (f1)))
    (coll-reduce [_ f1 init]
      (coll-reduce reducible (transformf f1) init))))

(defn my-map [mapf reducible]
  (make-reducer reducible
                (fn [reducef]
                  (fn [acc v]
                    (reducef acc (mapf v))))))

(defn my-flatten [reducible]
  (make-reducer reducible
                (fn [reducef]
                  (fn [acc v]
                    (if (sequential? v)
                      (coll-reduce (my-flatten v) reducef acc)
                      (reducef acc v))))))

(defn my-filter [filterf reducible]
  (make-reducer reducible
                (fn [reducef]
                  (fn [acc v]
                    (if (filterf v)
                      (reducef acc v)
                      acc)))))