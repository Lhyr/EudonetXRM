 /*
 A la monté de version, si le paramètre ADFS_LOGIN_WITHOUTDOMAIN n'est pas set,
 on le force à 0 si on est en mode auto-create.
  
En effet, dans ce cas, l'adfs retourne par défaut le login de l'utilisateur avec le domaine  
 
 */
 
 IF (
		(
			SELECT count(1)
			FROM configadv
			WHERE parameter = 'ADFS_LOGIN_WITHOUTDOMAIN'
			) = 0
		AND (
			SELECT count(1)
			FROM configadv
			WHERE parameter = 'ADFS_AUTOCREATE'
				AND VALUE = '1'
			) = 1
		)
	INSERT INTO configadv ( parameter,value) SELECT 'ADFS_LOGIN_WITHOUTDOMAIN' ,'0'
