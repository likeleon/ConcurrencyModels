(ns reducers.core
  (:require [clojure.core.protocols :refer [CollReduce coll-reduce]]
            [clojure.core.reducers :refer [CollFold coll-fold]]))

(defn my-reduce
  ([f coll] (coll-reduce coll f))
  ([f init coll] (coll-reduce coll f init)))

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

(print (into [] (my-map (partial * 2) (my-map (partial + 1) [1 2 3 4]))))