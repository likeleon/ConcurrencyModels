(ns reducers.core
  (:require [clojure.core.protocols :refer [CollReduce coll-reduce]]
            [clojure.core.reducers :refer [CollFold coll-fold]]))

(defn my-reduce
  ([f coll] (coll-reduce coll f))
  ([f init coll] (coll-reduce coll f init)))

(defn make-reducer [reducible transformf]
  (reify
    CollFold
    (coll-fold [_ n combinef reducef]
      (coll-fold reducible n combinef (transformf reducef)))

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

(defn my-fold
  ([reducef coll]
   (my-fold reducef reducef coll))
  ([combinef reducef coll]
   (my-fold 512 combinef reducef coll))
  ([n combinef reducef coll]
   (coll-fold coll n combinef reducef)))

(print (into [] (my-map (partial * 2) (my-map (partial + 1) [1 2 3 4]))))

(def v (into [] (range 1000000)))
(time (my-reduce + v))
(time (my-fold + v))